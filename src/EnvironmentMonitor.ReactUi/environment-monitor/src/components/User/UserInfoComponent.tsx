import React from "react";
import {
  Box,
  Paper,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
} from "@mui/material";
import { Google, Microsoft, GitHub, DeleteForever } from "@mui/icons-material";
import type { User } from "../../models/user";
import type { Device } from "../../models/device";
import type { LocationModel } from "../../models/location";
import { ChangePasswordComponent } from "./ChangePasswordComponent";
import { Collapsible } from "../CollabsibleComponent";
import { RoleNames } from "../../enums/roleNames";
import { stringSort } from "../../utilities/stringUtils";

export interface UserInfoComponentProps {
  user: User;
  isLoading: boolean;
  onChangePassword: (
    currentPassword: string,
    newPassword: string
  ) => Promise<void>;
  onRemove: (user: User) => void;
  elevation?: boolean;
  devices?: Device[];
  locations?: LocationModel[];
}

export const UserInfoComponent: React.FC<UserInfoComponentProps> = ({
  user,
  isLoading,
  onChangePassword,
  onRemove,
  devices = [],
  locations = [],
}) => {
  const authProvider = user.authenticationProvider || "Internal";
  const canChangePassword = !user.authenticationProvider;

  // Check if user has Admin or Viewer role
  const hasAdminOrViewerRole = user.roles.some(
    (role) => role === RoleNames.Admin || role === RoleNames.Viewer
  );

  const getProviderIcon = () => {
    if (!user.authenticationProvider) return null;

    const provider = user.authenticationProvider.toLowerCase();

    if (provider === "google") {
      return <Google sx={{ mr: 1, verticalAlign: "middle", fontSize: 24 }} />;
    }
    if (provider === "microsoft") {
      return (
        <Microsoft sx={{ mr: 1, verticalAlign: "middle", fontSize: 24 }} />
      );
    }
    if (provider === "github") {
      return <GitHub sx={{ mr: 1, verticalAlign: "middle", fontSize: 24 }} />;
    }
    return null;
  };

  return (
    <Box sx={{ p: 2 }}>
      <Collapsible title="User Information" isOpen={true}>
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
      </Collapsible>

      {/* Show available devices and locations if user doesn't have Admin or Viewer role */}
      {!hasAdminOrViewerRole &&
        (devices.length > 0 || locations.length > 0) && (
          <Collapsible title="Available Access" isOpen={false}>
            <Box sx={{ mt: 3 }}>
              {devices.length > 0 && (
                <Box sx={{ mb: 2 }}>
                  <Typography
                    variant="subtitle2"
                    color="text.secondary"
                    gutterBottom
                  >
                    Devices
                  </Typography>
                  <TableContainer component={Paper} variant="outlined">
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Device Name</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {[...devices]
                          .sort((a, b) =>
                            stringSort(a.displayName, b.displayName)
                          )
                          .map((device, index) => (
                            <TableRow key={index}>
                              <TableCell>
                                <Tooltip
                                  title={`Identifier: ${device.identifier}`}
                                  arrow
                                >
                                  <span style={{ cursor: "help" }}>
                                    {device.displayName || device.name}
                                  </span>
                                </Tooltip>
                              </TableCell>
                            </TableRow>
                          ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </Box>
              )}

              {/* Locations */}
              {locations.length > 0 && (
                <Box sx={{ mb: 2 }}>
                  <Typography
                    variant="subtitle2"
                    color="text.secondary"
                    gutterBottom
                  >
                    Locations
                  </Typography>
                  <TableContainer component={Paper} variant="outlined">
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Location Name</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {locations
                          .sort((a, b) => stringSort(a.name, b.name))
                          .map((location, index) => (
                            <TableRow key={index}>
                              <TableCell>
                                <Tooltip
                                  title={`Identifier: ${location.identifier}`}
                                  arrow
                                >
                                  <span style={{ cursor: "help" }}>
                                    {location.name}
                                  </span>
                                </Tooltip>
                              </TableCell>
                            </TableRow>
                          ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </Box>
              )}
            </Box>
          </Collapsible>
        )}

      {canChangePassword && (
        <Collapsible title="Change Password" isOpen={false}>
          <ChangePasswordComponent
            isLoading={isLoading}
            onChangePassword={onChangePassword}
          />
        </Collapsible>
      )}

      <Collapsible title="Delete Account" isOpen={false}>
        <Box sx={{ p: 2 }}>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            This action cannot be undone.
          </Typography>
          <Button
            variant="contained"
            color="error"
            startIcon={<DeleteForever />}
            onClick={() => onRemove(user)}
            disabled={isLoading}
          >
            Delete Account
          </Button>
        </Box>
      </Collapsible>
    </Box>
  );
};
