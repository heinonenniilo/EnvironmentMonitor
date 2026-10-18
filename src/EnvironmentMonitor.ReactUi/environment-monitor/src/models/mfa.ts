import type { MfaMethod } from "./user";

export interface LoginResultDto {
  success: boolean;
  requiresTwoFactor: boolean;
  availableMethods: MfaMethod[];
  message?: string;
}

export interface MfaSetupDto {
  method: MfaMethod;
  sharedKey?: string;
  authenticatorUri?: string;
  codeSent: boolean;
}

export interface MfaChallengeDto {
  challengePending: boolean;
  availableMethods: MfaMethod[];
  loginProvider?: string;
  maskedEmail?: string;
}
