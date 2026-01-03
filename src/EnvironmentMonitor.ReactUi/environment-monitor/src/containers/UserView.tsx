import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router";
import {
  Box,
  Card,
  CardContent,
  Chip,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from "@mui/material";
import { Google, Microsoft, GitHub, Edit } from "@mui/icons-material";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useApiHook } from "../hooks/apiHook";
import type { UserInfoDto, UserClaimDto } from "../models/userInfoDto";
import { useDispatch, useSelector } from "react-redux";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";
import { routes } from "../utilities/routes";
import { ManageRolesDialog } from "../components/UserManagement/ManageRolesDialog";
import { ManageClaimsDialog } from "../components/UserManagement/ManageClaimsDialog";
import { UserActionsComponent } from "../components/UserManagement/UserActionsComponent";
import { getDevices, getLocations } from "../reducers/measurementReducer";

const getProviderIcon = (provider: string) => {
  const providerLower = provider.toLowerCase();
  if (providerLower.includes("google")) {
    return <Google />;
  } else if (providerLower.includes("microsoft")) {
    return <Microsoft />;
  } else if (providerLower.includes("github")) {
    return <GitHub />;
  }
  return null;
};

export const UserView: React.FC = () => {
  const [user, setUser] = useState<UserInfoDto | undefined>(undefined);
  const [isLoading, setIsLoading] = useState(false);
  const [rolesDialogOpen, setRolesDialogOpen] = useState(false);
  const [claimsDialogOpen, setClaimsDialogOpen] = useState(false);
  const { userId } = useParams<{ userId?: string }>();
  const navigate = useNavigate();
  const userManagementHook = useApiHook().userManagementHook;
  const dispatch = useDispatch();

  const locations = useSelector(getLocations);
  const devices = useSelector(getDevices);

  // Helper function to get display value for claims
  const getClaimDisplayValue = (claim: UserClaimDto): string => {
    const claimTypeLower = claim.type.toLowerCase();

    if (claimTypeLower === "location") {
      const location = locations.find(
        (l) => l.identifier.toLowerCase() === claim.value.toLowerCase()
      );
      return location ? location.name : claim.value;
    }

    if (claimTypeLower === "device") {
      const device = devices.find(
        (d) => d.identifier.toLowerCase() === claim.value.toLowerCase()
      );
      return device ? device.displayName || device.name : claim.value;
    }

    return claim.value;
  };

  useEffect(() => {
    if (userId) {
      loadUser(userId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userId]);

  const loadUser = (id: string) => {
    setIsLoading(true);
    userManagementHook
      .getUser(id)
      .then((res) => {
        if (res) {
          setUser(res);
        } else {
          dispatch(
            addNotification({
              title: "User not found",
              body: "",
              severity: "error",
            })
          );
          navigate(routes.users);
        }
      })
      .catch((error) => {
        console.error(error);
        dispatch(
          addNotification({
            title: "Failed to load user",
            body: "",
            severity: "error",
          })
        );
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleDeleteUser = () => {
    if (!user || !userId) return;

    dispatch(
      setConfirmDialog({
        onConfirm: () => {
          setIsLoading(true);
          userManagementHook
            .deleteUser(userId)
            .then((success) => {
              if (success) {
                dispatch(
                  addNotification({
                    title: "User deleted successfully",
                    body: `User ${user.email} has been deleted.`,
                    severity: "success",
                  })
                );
                navigate(routes.users);
              } else {
                dispatch(
                  addNotification({
                    title: "Failed to delete user",
                    body: "",
                    severity: "error",
                  })
                );
              }
            })
            .catch((error) => {
              console.error(error);
              dispatch(
                addNotification({
                  title: "Failed to delete user",
                  body: "",
                  severity: "error",
                })
              );
            })
            .finally(() => {
              setIsLoading(false);
            });
        },
        title: "Delete User",
        body: `Are you sure you want to delete user ${user.email}? This action cannot be undone.`,
      })
    );
  };

  const handleManageRoles = (rolesToAdd: string[], rolesToRemove: string[]) => {
    if (!userId) return;

    setIsLoading(true);
    userManagementHook
      .manageUserRoles({
        userId,
        rolesToAdd,
        rolesToRemove,
      })
      .then((success) => {
        if (success) {
          dispatch(
            addNotification({
              title: "Roles updated successfully",
              body: "",
              severity: "success",
            })
          );
          loadUser(userId);
        } else {
          dispatch(
            addNotification({
              title: "Failed to update roles",
              body: "",
              severity: "error",
            })
          );
        }
      })
      .catch((error) => {
        console.error(error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleManageClaims = (
    claimsToAdd: UserClaimDto[],
    claimsToRemove: UserClaimDto[]
  ) => {
    if (!userId) return;

    setIsLoading(true);
    userManagementHook
      .manageUserClaims({
        userId,
        claimsToAdd,
        claimsToRemove,
      })
      .then((success) => {
        if (success) {
          dispatch(
            addNotification({
              title: "Claims updated successfully",
              body: "",
              severity: "success",
            })
          );
          loadUser(userId);
        } else {
          dispatch(
            addNotification({
              title: "Failed to update claims",
              body: "",
              severity: "error",
            })
          );
        }
      })
      .catch((error) => {
        console.error(error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  if (!user && !isLoading) {
    return (
      <AppContentWrapper title="User Not Found" isLoading={false}>
        <Typography>User not found</Typography>
      </AppContentWrapper>
    );
  }

  return (
    <AppContentWrapper
      title={user?.email || "User"}
      isLoading={isLoading}
      titleComponent={
        <UserActionsComponent
          onBack={() => navigate(routes.users)}
          onDelete={handleDeleteUser}
        />
      }
    >
      {user && (
        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" },
            gap: 3,
          }}
        >
          {/* Basic Information */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Basic Information
              </Typography>
              <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    User ID
                  </Typography>
                  <Typography variant="body1">{user.id}</Typography>
                </Box>
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Email
                  </Typography>
                  <Typography variant="body1">{user.email}</Typography>
                </Box>
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Username
                  </Typography>
                  <Typography variant="body1">
                    {user.userName || "N/A"}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Email Confirmed
                  </Typography>
                  <Chip
                    label={user.emailConfirmed ? "Yes" : "No"}
                    size="small"
                    color={user.emailConfirmed ? "success" : "warning"}
                    sx={{ mt: 0.5 }}
                  />
                </Box>
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    External Logins
                  </Typography>
                  {user.externalLogins && user.externalLogins.length > 0 ? (
                    <Box
                      sx={{
                        display: "flex",
                        gap: 1,
                        flexWrap: "wrap",
                        mt: 0.5,
                      }}
                    >
                      {user.externalLogins.map((login, index) => (
                        <Chip
                          key={index}
                          icon={
                            getProviderIcon(login.loginProvider) || undefined
                          }
                          label={login.loginProvider}
                          size="small"
                          variant="outlined"
                        />
                      ))}
                    </Box>
                  ) : (
                    <Typography
                      variant="body2"
                      color="text.secondary"
                      sx={{ mt: 0.5 }}
                    >
                      None
                    </Typography>
                  )}
                </Box>
              </Box>
            </CardContent>
          </Card>

          {/* Roles & Claims */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Roles & Claims
              </Typography>
              <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
                {/* Roles Section */}
                <Box>
                  <Box
                    sx={{
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "space-between",
                      mb: 1,
                    }}
                  >
                    <Typography variant="subtitle2" color="text.secondary">
                      Roles
                    </Typography>
                    <IconButton
                      size="small"
                      color="primary"
                      onClick={() => setRolesDialogOpen(true)}
                      title="Manage Roles"
                    >
                      <Edit fontSize="small" />
                    </IconButton>
                  </Box>
                  {user.roles.length > 0 ? (
                    <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap" }}>
                      {user.roles.map((role) => (
                        <Chip key={role} label={role} color="primary" />
                      ))}
                    </Box>
                  ) : (
                    <Typography variant="body2" color="text.secondary">
                      No roles assigned
                    </Typography>
                  )}
                </Box>

                {/* Claims Section */}
                <Box>
                  <Box
                    sx={{
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "space-between",
                      mb: 1,
                    }}
                  >
                    <Typography variant="subtitle2" color="text.secondary">
                      Claims
                    </Typography>
                    <IconButton
                      size="small"
                      color="primary"
                      onClick={() => setClaimsDialogOpen(true)}
                      title="Manage Claims"
                    >
                      <Edit fontSize="small" />
                    </IconButton>
                  </Box>
                  {user.claims.length > 0 ? (
                    <TableContainer component={Paper} variant="outlined">
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Type</TableCell>
                            <TableCell>Value</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {user.claims.map((claim, index) => (
                            <TableRow key={index}>
                              <TableCell>{claim.type}</TableCell>
                              <TableCell>
                                <Tooltip
                                  title={`Identifier: ${claim.value}`}
                                  arrow
                                >
                                  <span style={{ cursor: "help" }}>
                                    {getClaimDisplayValue(claim)}
                                  </span>
                                </Tooltip>
                              </TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  ) : (
                    <Typography variant="body2" color="text.secondary">
                      No claims assigned
                    </Typography>
                  )}
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Box>
      )}

      {user && userId && (
        <>
          <ManageRolesDialog
            open={rolesDialogOpen}
            onClose={() => setRolesDialogOpen(false)}
            onSave={handleManageRoles}
            currentRoles={user.roles}
            userId={userId}
          />
          <ManageClaimsDialog
            open={claimsDialogOpen}
            onClose={() => setClaimsDialogOpen(false)}
            onSave={handleManageClaims}
            currentClaims={user.claims}
            userId={userId}
          />
        </>
      )}
    </AppContentWrapper>
  );
};
