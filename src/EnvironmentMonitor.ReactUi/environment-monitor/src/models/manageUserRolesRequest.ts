export interface ManageUserRolesRequest {
  userId: string;
  rolesToAdd?: string[];
  rolesToRemove?: string[];
}
