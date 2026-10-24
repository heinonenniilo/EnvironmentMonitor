import type { MfaMethod } from "./user";

export interface UserClaimDto {
  type: string;
  value: string;
}

export interface ExternalLoginInfoDto {
  loginProvider: string;
}

export interface UserInfoDto {
  id: string;
  email: string;
  userName?: string;
  emailConfirmed: boolean;
  mfaMethods: MfaMethod[];
  roles: string[];
  claims: UserClaimDto[];
  externalLogins: ExternalLoginInfoDto[];
  lockoutEnd?: Date | null;
  lockoutEnabled: boolean;
  accessFailedCount: number;
  updated?: Date;
  updatedById?: string;
}
