export enum MfaMethod {
  Email = 0,
  Authenticator = 1,
}

export interface MfaStatus {
  twoFactorEnabled: boolean;
  emailEnabled: boolean;
  authenticatorEnabled: boolean;
  enabledMethods: MfaMethod[];
}

export interface User {
  id: string;
  email: string;
  emailConfirmed: boolean;
  loggedIn: boolean;
  roles: string[];
  authenticationProvider?: string;
  upnExternal?: string;
  mfa?: MfaStatus;
}

export interface ConfirmEmailCodeRequest {
  code: string;
}

export interface ConfirmEmailCodeResult {
  message: string;
  emailConfirmed: boolean;
}
