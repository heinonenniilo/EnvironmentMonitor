import type { UserClaimDto } from "./userInfoDto";

export interface ManageUserClaimsRequest {
  userId: string;
  claimsToAdd?: UserClaimDto[];
  claimsToRemove?: UserClaimDto[];
}
