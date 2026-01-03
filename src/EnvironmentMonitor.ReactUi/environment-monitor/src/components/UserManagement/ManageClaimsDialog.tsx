import React, { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Chip,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  Tooltip,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
} from "@mui/material";
import { Delete } from "@mui/icons-material";
import type { UserClaimDto } from "../../models/userInfoDto";
import { useSelector } from "react-redux";
import { getDevices, getLocations } from "../../reducers/measurementReducer";
import { stringSort } from "../../utilities/stringUtils";

export interface ManageClaimsDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (claimsToAdd: UserClaimDto[], claimsToRemove: UserClaimDto[]) => void;
  currentClaims: UserClaimDto[];
  userId: string;
}

export const ManageClaimsDialog: React.FC<ManageClaimsDialogProps> = ({
  open,
  onClose,
  onSave,
  currentClaims,
}) => {
  const [newClaimType, setNewClaimType] = useState("");
  const [newClaimValue, setNewClaimValue] = useState("");
  const [claimsToAdd, setClaimsToAdd] = useState<UserClaimDto[]>([]);
  const [claimsToRemove, setClaimsToRemove] = useState<UserClaimDto[]>([]);

  const locations = useSelector(getLocations);
  const devices = useSelector(getDevices);

  const claimTypes = [
    { value: "Device", label: "Device" },
    { value: "Location", label: "Location" },
  ];

  // Determine if current claim type needs a dropdown
  const isLocationClaim = newClaimType === "Location";
  const isDeviceClaim = newClaimType === "Device";

  // Helper function to get display value for claims
  const getClaimDisplayValue = (claim: UserClaimDto): string => {
    if (claim.type === "Location") {
      const location = locations.find(
        (l) => l.identifier.toLowerCase() === claim.value.toLowerCase()
      );
      return location ? location.name : claim.value;
    }

    if (claim.type === "Device") {
      const device = devices.find(
        (d) => d.identifier.toLowerCase() === claim.value.toLowerCase()
      );
      return device ? device.displayName || device.name : claim.value;
    }

    return claim.value;
  };

  const handleAddClaim = () => {
    if (newClaimType.trim() && newClaimValue.trim()) {
      const newClaim = {
        type: newClaimType.trim(),
        value: newClaimValue.trim(),
      };
      setClaimsToAdd([...claimsToAdd, newClaim]);
      setNewClaimType("");
      setNewClaimValue("");
    }
  };

  const handleRemoveFromAdding = (index: number) => {
    setClaimsToAdd(claimsToAdd.filter((_, i) => i !== index));
  };

  const handleMarkForRemoval = (claim: UserClaimDto) => {
    const exists = claimsToRemove.some(
      (c) => c.type === claim.type && c.value === claim.value
    );
    if (!exists) {
      setClaimsToRemove([...claimsToRemove, claim]);
    }
  };

  const handleUnmarkForRemoval = (claim: UserClaimDto) => {
    setClaimsToRemove(
      claimsToRemove.filter(
        (c) => !(c.type === claim.type && c.value === claim.value)
      )
    );
  };

  const isMarkedForRemoval = (claim: UserClaimDto) => {
    return claimsToRemove.some(
      (c) => c.type === claim.type && c.value === claim.value
    );
  };

  const handleSave = () => {
    onSave(claimsToAdd, claimsToRemove);
    setNewClaimType("");
    setNewClaimValue("");
    setClaimsToAdd([]);
    setClaimsToRemove([]);
    onClose();
  };

  const handleCancel = () => {
    setNewClaimType("");
    setNewClaimValue("");
    setClaimsToAdd([]);
    setClaimsToRemove([]);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleCancel} maxWidth="md" fullWidth>
      <DialogTitle>Manage User Claims</DialogTitle>
      <DialogContent>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 3, pt: 1 }}>
          {/* Current Claims */}
          <Box>
            <Typography variant="subtitle2" gutterBottom>
              Current Claims
            </Typography>
            {currentClaims.length > 0 ? (
              <TableContainer component={Paper} variant="outlined">
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Type</TableCell>
                      <TableCell>Value</TableCell>
                      <TableCell width={60}>Action</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {currentClaims.map((claim, index) => (
                      <TableRow
                        key={index}
                        sx={{
                          textDecoration: isMarkedForRemoval(claim)
                            ? "line-through"
                            : "none",
                          backgroundColor: isMarkedForRemoval(claim)
                            ? "action.hover"
                            : "inherit",
                        }}
                      >
                        <TableCell>{claim.type}</TableCell>
                        <TableCell>
                          <Tooltip title={`Identifier: ${claim.value}`} arrow>
                            <span style={{ cursor: "help" }}>
                              {getClaimDisplayValue(claim)}
                            </span>
                          </Tooltip>
                        </TableCell>
                        <TableCell>
                          <IconButton
                            size="small"
                            color={
                              isMarkedForRemoval(claim) ? "default" : "error"
                            }
                            onClick={() =>
                              isMarkedForRemoval(claim)
                                ? handleUnmarkForRemoval(claim)
                                : handleMarkForRemoval(claim)
                            }
                          >
                            <Delete fontSize="small" />
                          </IconButton>
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

          {/* Add New Claim */}
          <Box>
            <Typography variant="subtitle2" gutterBottom>
              Add New Claim
            </Typography>
            <Box sx={{ display: "flex", gap: 1, mb: 1 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Claim Type</InputLabel>
                <Select
                  value={newClaimType}
                  label="Claim Type"
                  onChange={(e) => {
                    setNewClaimType(e.target.value);
                    setNewClaimValue(""); // Reset value when type changes
                  }}
                >
                  {claimTypes.map((type) => (
                    <MenuItem key={type.value} value={type.value}>
                      {type.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl fullWidth size="small" disabled={!newClaimType}>
                <InputLabel>
                  {isLocationClaim
                    ? "Location"
                    : isDeviceClaim
                      ? "Device"
                      : "Value"}
                </InputLabel>
                <Select
                  value={newClaimValue}
                  label={
                    isLocationClaim
                      ? "Location"
                      : isDeviceClaim
                        ? "Device"
                        : "Value"
                  }
                  onChange={(e) => setNewClaimValue(e.target.value)}
                >
                  {isLocationClaim &&
                    [...locations]
                      .sort((a, b) => stringSort(a.name, b.name))
                      .map((location) => (
                        <MenuItem
                          key={location.identifier}
                          value={location.identifier}
                        >
                          {location.name}
                        </MenuItem>
                      ))}
                  {isDeviceClaim &&
                    [...devices]
                      .sort((a, b) =>
                        stringSort(
                          a.displayName || a.name,
                          b.displayName || b.name
                        )
                      )
                      .map((device) => (
                        <MenuItem
                          key={device.identifier}
                          value={device.identifier}
                        >
                          {device.displayName || device.name}
                        </MenuItem>
                      ))}
                </Select>
              </FormControl>

              <Button
                variant="contained"
                onClick={handleAddClaim}
                disabled={!newClaimType || !newClaimValue}
              >
                Add
              </Button>
            </Box>
            {claimsToAdd.length > 0 && (
              <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap" }}>
                {claimsToAdd.map((claim, index) => (
                  <Tooltip
                    key={index}
                    title={`${claim.type}: ${claim.value}`}
                    arrow
                  >
                    <Chip
                      label={`${claim.type}: ${getClaimDisplayValue(claim)}`}
                      color="success"
                      onDelete={() => handleRemoveFromAdding(index)}
                    />
                  </Tooltip>
                ))}
              </Box>
            )}
          </Box>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleCancel}>Cancel</Button>
        <Button
          onClick={handleSave}
          variant="contained"
          disabled={claimsToAdd.length === 0 && claimsToRemove.length === 0}
        >
          Save Changes
        </Button>
      </DialogActions>
    </Dialog>
  );
};
