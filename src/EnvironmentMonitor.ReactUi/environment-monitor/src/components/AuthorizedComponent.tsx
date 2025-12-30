import React from "react";
import { useSelector } from "react-redux";
import { RoleNames } from "../enums/roleNames";
import { getUserInfo } from "../reducers/userReducer";

export interface AuthorizedComponentProps {
  requiredRole?: RoleNames;
  requiredRoles?: RoleNames[];
  roleLogic?: "AND" | "OR";
  children: React.ReactNode;
}

export const AuthorizedComponent: React.FunctionComponent<
  AuthorizedComponentProps
> = ({ requiredRole, requiredRoles, roleLogic = "OR", children }) => {
  const user = useSelector(getUserInfo);

  const isAdmin = user?.roles?.some((r) => r === RoleNames.Admin);

  if (isAdmin) {
    return <>{children}</>;
  }

  const rolesToCheck = requiredRoles ?? (requiredRole ? [requiredRole] : []);

  // If no roles specified, deny access
  if (rolesToCheck.length === 0) {
    return <></>;
  }

  // Check role logic
  const hasAccess =
    roleLogic === "AND"
      ? rolesToCheck.every((role) => user?.roles?.includes(role))
      : rolesToCheck.some((role) => user?.roles?.includes(role));

  if (hasAccess) {
    return <>{children}</>;
  } else {
    return <></>;
  }
};
