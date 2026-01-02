export interface UserClaimDto {
  type: string;
  value: string;
}

export interface UserInfoDto {
  id: string;
  email: string;
  userName?: string;
  emailConfirmed: boolean;
  roles: string[];
  claims: UserClaimDto[];
}
