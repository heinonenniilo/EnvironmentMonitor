import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { createRequire } from "node:module";
import { dirname, resolve } from "node:path";
import test from "node:test";
import vm from "node:vm";
import ts from "typescript";

const require = createRequire(import.meta.url);
const root = resolve(import.meta.dirname, "..");

// Exercise the existing TypeScript modules without adding a test framework.
function loadModule(path, overrides = {}, globals = {}) {
  const filename = resolve(root, path);
  const source = ts.transpileModule(readFileSync(filename, "utf8"), {
    compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2020 },
  }).outputText;
  const module = { exports: {} };
  const localRequire = (name) => {
    if (Object.hasOwn(overrides, name)) return overrides[name];
    if (name.startsWith(".")) return loadModule(resolve(dirname(filename), `${name}.ts`), overrides, globals);
    return require(name);
  };
  vm.runInNewContext(source, { module, exports: module.exports, require: localRequire, console, ...globals }, { filename });
  return module.exports;
}

function apiFixture(response, failure) {
  const calls = [];
  const request = async (verb, url, body) => {
    calls.push({ verb, url, body });
    if (failure) throw failure;
    return { data: response };
  };
  const { useApiHook } = loadModule("src/hooks/apiHook.ts", {
    axios: { default: { create: () => ({ get: (url) => request("GET", url), post: (url, body) => request("POST", url, body) }) } },
    "./useNotification": { useNotification: () => ({ error() {} }) },
  });
  return { api: useApiHook().userHook, calls };
}

test("password login preserves the MFA-required result and remember-me payload", async () => {
  const result = { success: false, requiresTwoFactor: true, availableMethods: [0, 1] };
  const { api, calls } = apiFixture(result);
  assert.equal(await api.logIn("user@example.com", "password", true), result);
  assert.equal(calls[0].url, "/api/authentication/login");
  assert.equal(JSON.stringify(calls[0].body), JSON.stringify({ userName: "user@example.com", password: "password", persistent: true }));
});

test("MFA login sends numeric methods, codes including leading zeros, and persistence", async () => {
  const result = { success: true, requiresTwoFactor: false, availableMethods: [] };
  const { api, calls } = apiFixture(result);
  assert.equal(await api.completeMfaLogin(0, "012345", true), result);
  assert.equal(calls[0].url, "/api/authentication/mfa/login");
  assert.equal(JSON.stringify(calls[0].body), JSON.stringify({ method: 0, code: "012345", persistent: true }));
});

test("challenge loading and email delivery use the anonymous challenge routes", async () => {
  const result = { challengePending: true, availableMethods: [0, 1] };
  const { api, calls } = apiFixture(result);
  assert.equal(await api.getMfaChallenge(), result);
  await api.sendMfaEmailCode();
  assert.deepEqual(calls.map(({ verb, url }) => ({ verb, url })), [
    { verb: "GET", url: "/api/authentication/mfa/challenge" },
    { verb: "POST", url: "/api/authentication/mfa/challenge/send-email-code" },
  ]);
  assert.equal(calls[1].body, undefined);
});

test("enrollment, confirmation, and per-method disabling use the management contracts", async () => {
  const status = { twoFactorEnabled: true, enabledMethods: [0, 1], emailEnabled: true, authenticatorEnabled: true };
  const { api, calls } = apiFixture(status);
  await api.getMfaStatus();
  await api.setupMfa(1);
  assert.equal(await api.confirmMfa(1, "012345"), status);
  assert.equal(await api.disableMfa(0), status);
  await api.disableMfa(1, "654321");
  assert.deepEqual(calls.map((call) => call.url), ["/api/authentication/mfa", "/api/authentication/mfa/setup", "/api/authentication/mfa/confirm", "/api/authentication/mfa/disable", "/api/authentication/mfa/disable"]);
  assert.equal(JSON.stringify(calls[1].body), '{"method":1}');
  assert.equal(JSON.stringify(calls[2].body), '{"method":1,"code":"012345"}');
  assert.equal(JSON.stringify(calls[3].body), '{"method":0}');
  assert.equal(JSON.stringify(calls[4].body), '{"method":1,"code":"654321"}');
});

test("invalid codes reject with the backend message available to the UI", async () => {
  const failure = { isAxiosError: true, response: { data: { message: "Code expired" } } };
  const { api } = apiFixture(undefined, failure);
  await assert.rejects(api.confirmMfa(0, "123456"), (error) => error === failure);
  await assert.rejects(api.completeMfaLogin(0, "123456", false), (error) => error === failure);
  const { mfaErrorMessage } = loadModule("src/utilities/mfaUtils.ts");
  assert.equal(mfaErrorMessage(failure, "Fallback"), "Code expired");
});

test("MFA state updates preserve the signed-in user and never create an authenticated session", () => {
  const { default: reducer, storeUserInfo, storeMfaStatus } = loadModule("src/reducers/userReducer.ts");
  const status = { twoFactorEnabled: true, enabledMethods: [1], emailEnabled: false, authenticatorEnabled: true };
  const anonymous = reducer(undefined, storeMfaStatus(status));
  assert.equal(anonymous.user, undefined);
  const signedIn = reducer(anonymous, storeUserInfo({ email: "user@example.com", roles: ["User"], loggedIn: true }));
  const updated = reducer(signedIn, storeMfaStatus(status));
  assert.equal(updated.user.email, signedIn.user.email);
  assert.deepEqual(updated.user.mfa, status);
});

test("remember-me survives a new page instance, clears after login, and tolerates unavailable storage", () => {
  const values = new Map();
  const sessionStorage = { setItem: (key, value) => values.set(key, value), getItem: (key) => values.get(key), removeItem: (key) => values.delete(key) };
  const firstPage = loadModule("src/utilities/mfaUtils.ts", {}, { sessionStorage });
  firstPage.rememberLoginPersistence(true);
  const challengePage = loadModule("src/utilities/mfaUtils.ts", {}, { sessionStorage });
  assert.equal(challengePage.readLoginPersistence(), true);
  challengePage.clearLoginPersistence();
  assert.equal(challengePage.readLoginPersistence(), false);
  const unavailable = loadModule("src/utilities/mfaUtils.ts");
  assert.doesNotThrow(() => unavailable.rememberLoginPersistence(true));
  assert.equal(unavailable.readLoginPersistence(), false);
});
