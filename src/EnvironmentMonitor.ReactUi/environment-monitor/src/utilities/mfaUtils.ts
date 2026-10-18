import { isAxiosError } from "axios";
import { MfaMethod } from "../models/user";

export const mfaMethodLabel = (method: MfaMethod): string =>
  method === MfaMethod.Email ? "Email" : "Authenticator app";

export const mfaErrorMessage = (error: unknown, fallback: string): string => {
  if (isAxiosError<{ message?: string; title?: string; detail?: string }>(error)) {
    const data = error.response?.data;
    return data?.title || data?.detail || data?.message || fallback;
  }
  return error instanceof Error ? error.message : fallback;
};

// Session storage survives both page refreshes and external provider redirects.
const persistentKey = "mfa-login-persistent";
export const rememberLoginPersistence = (persistent: boolean) => {
  try { sessionStorage.setItem(persistentKey, String(persistent)); } catch { /* Storage may be unavailable. */ }
};
export const readLoginPersistence = (): boolean => {
  try { return sessionStorage.getItem(persistentKey) === "true"; } catch { return false; }
};
export const clearLoginPersistence = () => {
  try { sessionStorage.removeItem(persistentKey); } catch { /* Storage may be unavailable. */ }
};
