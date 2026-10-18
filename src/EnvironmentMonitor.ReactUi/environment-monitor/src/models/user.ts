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
  email: string;
  loggedIn: boolean;
  roles: string[];
  authenticationProvider?: string;
  upnExternal?: string;
  mfa?: MfaStatus;
}
