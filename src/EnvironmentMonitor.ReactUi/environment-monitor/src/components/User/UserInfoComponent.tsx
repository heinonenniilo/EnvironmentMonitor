import React from "react";
import { Box, Paper, Typography, Button } from "@mui/material";
import { Google, Microsoft, DeleteForever } from "@mui/icons-material";
import type { User } from "../../models/user";
import { ChangePasswordComponent } from "./ChangePasswordComponent";
import { Collapsible } from "../CollabsibleComponent";

export interface UserInfoComponentProps {
  user: User;
  isLoading: boolean;
  onChangePassword: (
    currentPassword: string,
    newPassword: string
  ) => Promise<void>;
  onRemove: (user: User) => void;
  elevation?: boolean;
}

export const UserInfoComponent: React.FC<UserInfoComponentProps> = ({
  user,
  isLoading,
  onChangePassword,
  onRemove,
  elevation = false,
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
      <Paper elevation={elevation ? 3 : 0} sx={{ p: 3 }}>
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
        {user.upnExternal && (
          <Box sx={{ mb: 2 }}>
            <Typography variant="subtitle2" color="text.secondary">
              UPN (External):
            </Typography>
            <Box sx={{ display: "flex", alignItems: "center" }}>
              <Typography variant="body1">{user.upnExternal}</Typography>
            </Box>
          </Box>
        )}

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

      <Paper elevation={elevation ? 3 : 0} sx={{ p: 3 }}>
        <Typography variant="h6" color="error" gutterBottom>
          Danger Zone
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Once you delete your account, there is no going back. Please be
          certain.
        </Typography>
        <Button
          variant="outlined"
          color="error"
          startIcon={<DeleteForever />}
          onClick={() => onRemove(user)}
          disabled={isLoading}
        >
          Remove account
        </Button>
      </Paper>
    </Box>
  );
};
