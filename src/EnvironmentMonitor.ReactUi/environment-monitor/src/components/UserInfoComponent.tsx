import React from "react";
import { Box, Paper, Typography } from "@mui/material";
import { Google, Microsoft } from "@mui/icons-material";
import type { User } from "../models/user";
import { ChangePasswordComponent } from "./User/ChangePasswordComponent";
import { Collapsible } from "./CollabsibleComponent";

export interface UserInfoComponentProps {
  user: User;
  isLoading: boolean;
  onChangePassword: (
    currentPassword: string,
    newPassword: string
  ) => Promise<void>;
}

export const UserInfoComponent: React.FC<UserInfoComponentProps> = ({
  user,
  isLoading,
  onChangePassword,
}) => {
  const authProvider = user.authenticationProvider || "Internal";
  const canChangePassword = !user.authenticationProvider;

  const getProviderIcon = () => {
    if (user.authenticationProvider === "Google") {
      return <Google sx={{ mr: 1, verticalAlign: "middle" }} />;
    }
    if (user.authenticationProvider === "Microsoft") {
      return <Microsoft sx={{ mr: 1, verticalAlign: "middle" }} />;
    }
    return null;
  };

  return (
    <Box sx={{ p: 2 }}>
      <Paper elevation={3} sx={{ p: 3, mb: 2 }}>
        <Box sx={{ mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Email:
          </Typography>
          <Typography variant="body1">{user.email}</Typography>
        </Box>

        <Box sx={{ mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary">
            Authentication Provider:
          </Typography>
          <Box sx={{ display: "flex", alignItems: "center" }}>
            {getProviderIcon()}
            <Typography variant="body1">{authProvider}</Typography>
          </Box>
        </Box>

        <Box>
          <Typography variant="subtitle2" color="text.secondary">
            Roles:
          </Typography>
          <Typography variant="body1">
            {user.roles && user.roles.length > 0
              ? user.roles.join(", ")
              : "No roles assigned"}
          </Typography>
        </Box>
      </Paper>

      {canChangePassword && (
        <Collapsible title="Change Password" isOpen={false}>
          <ChangePasswordComponent
            isLoading={isLoading}
            onChangePassword={onChangePassword}
          />
        </Collapsible>
      )}
    </Box>
  );
};
