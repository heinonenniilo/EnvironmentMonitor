import assert from "node:assert/strict";
import { existsSync, readFileSync } from "node:fs";
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
    compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2020, jsx: ts.JsxEmit.ReactJSX },
  }).outputText;
  const module = { exports: {} };
  const localRequire = (name) => {
    if (Object.hasOwn(overrides, name)) return overrides[name];
    if (name.startsWith(".")) {
      const base = resolve(dirname(filename), name);
      return loadModule(existsSync(`${base}.ts`) ? `${base}.ts` : `${base}.tsx`, overrides, globals);
    }
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

test("email confirmation uses cookie-auth endpoints and preserves formatted codes", async () => {
  const result = { message: "Email confirmed.", emailConfirmed: true };
  const { api, calls } = apiFixture(result);
  await api.sendEmailConfirmationCode();
  assert.equal(await api.confirmEmailCode({ code: "012-345" }), result);
  assert.equal(calls[0].url, "/api/authentication/email/send-confirmation-code");
  assert.equal(calls[0].body, undefined);
  assert.equal(calls[1].url, "/api/authentication/email/confirm-code");
  assert.equal(JSON.stringify(calls[1].body), '{"code":"012-345"}');
});

test("email confirmation propagates both invalid-code errors and HTTP 500 ProblemDetails", async () => {
  const { mfaErrorMessage } = loadModule("src/utilities/mfaUtils.ts");
  for (const [status, data, expected] of [
    [400, { message: "The provided code is invalid or has expired." }, "The provided code is invalid or has expired."],
    [500, { title: "Email address must be confirmed before multi factor authentication can be enabled." }, "Email address must be confirmed before multi factor authentication can be enabled."],
    [500, { detail: "Email is already confirmed." }, "Email is already confirmed."],
    [500, {}, "Something went wrong."],
  ]) {
    const failure = { isAxiosError: true, response: { status, data } };
    const { api } = apiFixture(undefined, failure);
    await assert.rejects(api.confirmEmailCode({ code: "012345" }), (error) => error === failure);
    await assert.rejects(api.sendEmailConfirmationCode(), (error) => error === failure);
    assert.equal(mfaErrorMessage(failure, "Something went wrong."), expected);
  }
});

function settingsFixture(api) {
  const state = [];
  let cursor = 0;
  const actions = [];
  const { MfaSettings } = loadModule("src/components/User/MfaSettings.tsx", {
    react: { useState: (initial) => {
      const index = cursor++;
      if (!(index in state)) state[index] = initial;
      return [state[index], (value) => { state[index] = value; }];
    } },
    "react-redux": { useDispatch: () => (action) => actions.push(action) },
    "@mui/material": Object.fromEntries(["Alert", "Box", "Button", "Chip", "Dialog", "DialogActions", "DialogContent", "DialogTitle", "Stack", "Table", "TableBody", "TableCell", "TableContainer", "TableHead", "TableRow", "TextField", "Typography"].map((name) => [name, name])),
    "qrcode.react": { QRCodeSVG: "QRCodeSVG" },
    "../../hooks/apiHook": { useApiHook: () => ({ userHook: api }) },
    "../../hooks/useCooldown": { useCooldown: () => ({ seconds: 0, start() {} }) },
    "../../reducers/userReducer": { storeUserInfo: (payload) => ({ type: "user", payload }), storeMfaStatus: (payload) => ({ type: "mfa", payload }) },
    "./EmailConfirmationCode": { EmailConfirmationCode: "EmailConfirmationCode" },
    "./EmailCodeDialog": { EmailCodeDialog: "EmailCodeDialog" },
    "./AuthenticatorAppSetup": { AuthenticatorAppSetup: "AuthenticatorAppSetup" },
  });
  return {
    actions,
    render: (props) => { cursor = 0; return MfaSettings({ email: "user@example.com", ...props }); },
  };
}

function elements(tree, type) {
  if (Array.isArray(tree)) return tree.flatMap((child) => elements(child, type));
  if (!tree || typeof tree !== "object") return [];
  return [...(tree.type === type ? [tree] : []), ...elements(tree.props?.children, type)];
}

test("unconfirmed email blocks both setup methods while existing methods can still be disabled", async () => {
  let setups = 0;
  const fixture = settingsFixture({ setupMfa: async () => { setups++; } });
  for (const emailConfirmed of [false, undefined]) {
    const tree = fixture.render({ emailConfirmed });
    assert.equal(elements(tree, "EmailConfirmationCode").length, 1);
    const enables = elements(tree, "Button").filter((button) => button.props.children === "Enable");
    assert.equal(enables.length, 2);
    assert.ok(enables.every((button) => button.props.disabled));
    // Even a programmatic click cannot bypass the enrollment guard.
    enables.forEach((button) => button.props.onClick());
  }
  assert.equal(setups, 0);
  const tree = fixture.render({ emailConfirmed: false, status: { twoFactorEnabled: true, enabledMethods: [0] } });
  const disable = elements(tree, "Button").find((button) => button.props.children === "Disable");
  assert.equal(disable.props.disabled, false);
});

test("confirmed email exposes setup and stale enrollment errors refresh the user profile", async () => {
  const failure = { isAxiosError: true, response: { status: 500, data: { title: "Email address must be confirmed before multi factor authentication can be enabled." } } };
  const user = { email: "user@example.com", emailConfirmed: false };
  let refreshed = 0;
  const fixture = settingsFixture({
    setupMfa: async () => { throw failure; },
    getUserInfo: async () => { refreshed++; return user; },
  });
  const tree = fixture.render({ emailConfirmed: true });
  assert.equal(elements(tree, "EmailConfirmationCode").length, 0);
  const enable = elements(tree, "Button").find((button) => button.props.children === "Enable");
  assert.equal(enable.props.disabled, false);
  enable.props.onClick();
  await new Promise((resolve) => setImmediate(resolve));
  assert.equal(refreshed, 1);
  assert.equal(fixture.actions[0].payload, user);
  const refreshedTree = fixture.render({ emailConfirmed: false });
  assert.equal(elements(refreshedTree, "EmailConfirmationCode").length, 1);
  assert.ok(elements(refreshedTree, "Alert").some((alert) => alert.props.children === failure.response.data.title));
});

test("email MFA uses the shared dialog with setup and confirmation callbacks", async () => {
  const calls = [];
  const status = { twoFactorEnabled: true, emailEnabled: true, authenticatorEnabled: false, enabledMethods: [0] };
  const fixture = settingsFixture({
    setupMfa: async (method) => { calls.push(["setup", method]); },
    confirmMfa: async (method, code) => { calls.push(["confirm", method, code]); return status; },
  });
  const initial = fixture.render({ emailConfirmed: true });
  const enableButtons = elements(initial, "Button").filter((button) => button.props.children === "Enable");
  enableButtons[1].props.onClick();
  const dialog = elements(fixture.render({ emailConfirmed: true }), "EmailCodeDialog")[0];
  assert.equal(dialog.props.open, true);
  assert.equal(dialog.props.resendCooldown, 60);
  await dialog.props.onSend();
  await dialog.props.onConfirm("012345");
  assert.deepEqual(calls, [["setup", 0], ["confirm", 0, "012345"]]);
  assert.equal(fixture.actions[0].payload, status);
  dialog.props.onClose();
  assert.equal(elements(fixture.render({ emailConfirmed: true }), "EmailCodeDialog")[0].props.open, false);
});

test("email confirmation uses the same dialog without an email address prop", async () => {
  const calls = [];
  const { EmailConfirmationCode } = loadModule("src/components/User/EmailConfirmationCode.tsx", {
    react: { useState: () => [false, () => {}] },
    "@mui/material": { Alert: "Alert", Button: "Button", Stack: "Stack" },
    "../../hooks/apiHook": { useApiHook: () => ({ userHook: {
      sendEmailConfirmationCode: async () => { calls.push("send"); },
      confirmEmailCode: async ({ code }) => { calls.push(code); },
    } }) },
    "../../hooks/useNotification": { useNotification: () => ({ success() {} }) },
    "./EmailCodeDialog": { EmailCodeDialog: "EmailCodeDialog" },
  });
  const tree = EmailConfirmationCode({ onConfirmed: async () => { calls.push("refresh"); } });
  const dialog = elements(tree, "EmailCodeDialog")[0];
  assert.equal(dialog.props.title, "Confirm email");
  assert.equal(Object.hasOwn(dialog.props, "email"), false);
  await dialog.props.onSend();
  await dialog.props.onConfirm("012345");
  assert.deepEqual(calls, ["send", "012345", "refresh"]);
});
