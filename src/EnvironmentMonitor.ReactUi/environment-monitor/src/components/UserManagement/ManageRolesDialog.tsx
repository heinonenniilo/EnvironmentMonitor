import React, { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Chip,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from "@mui/material";

export interface ManageRolesDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (rolesToAdd: string[], rolesToRemove: string[]) => void;
  currentRoles: string[];
  userId: string;
}

export const ManageRolesDialog: React.FC<ManageRolesDialogProps> = ({
  open,
  onClose,
  onSave,
  currentRoles,
}) => {
  const [newRole, setNewRole] = useState("");
  const [rolesToAdd, setRolesToAdd] = useState<string[]>([]);
  const [rolesToRemove, setRolesToRemove] = useState<string[]>([]);

  const availableRoles = ["Viewer", "User", "Admin"];

  // Filter out roles that are already assigned or already added
  const selectableRoles = availableRoles.filter(
    (role) => !currentRoles.includes(role) && !rolesToAdd.includes(role)
  );

  const handleAddRole = () => {
    if (
      newRole.trim() &&
      !currentRoles.includes(newRole.trim()) &&
      !rolesToAdd.includes(newRole.trim())
    ) {
      setRolesToAdd([...rolesToAdd, newRole.trim()]);
      setNewRole("");
    }
  };

  const handleRemoveFromAdding = (role: string) => {
    setRolesToAdd(rolesToAdd.filter((r) => r !== role));
  };

  const handleMarkForRemoval = (role: string) => {
    if (!rolesToRemove.includes(role)) {
      setRolesToRemove([...rolesToRemove, role]);
    }
  };

  const handleUnmarkForRemoval = (role: string) => {
    setRolesToRemove(rolesToRemove.filter((r) => r !== role));
  };

  const handleSave = () => {
    onSave(rolesToAdd, rolesToRemove);
    setNewRole("");
    setRolesToAdd([]);
    setRolesToRemove([]);
    onClose();
  };

  const handleCancel = () => {
    setNewRole("");
    setRolesToAdd([]);
    setRolesToRemove([]);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleCancel} maxWidth="sm" fullWidth>
      <DialogTitle>Manage User Roles</DialogTitle>
      <DialogContent>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 3, pt: 1 }}>
          {/* Current Roles */}
          <Box>
            <Typography variant="subtitle2" gutterBottom>
              Current Roles
            </Typography>
            {currentRoles.length > 0 ? (
              <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap" }}>
                {currentRoles.map((role) => (
                  <Chip
                    key={role}
                    label={role}
                    color={rolesToRemove.includes(role) ? "default" : "primary"}
                    onDelete={
                      rolesToRemove.includes(role)
                        ? () => handleUnmarkForRemoval(role)
                        : () => handleMarkForRemoval(role)
                    }
                    sx={{
                      textDecoration: rolesToRemove.includes(role)
                        ? "line-through"
                        : "none",
                    }}
                  />
                ))}
              </Box>
            ) : (
              <Typography variant="body2" color="text.secondary">
                No roles assigned
              </Typography>
            )}
          </Box>

          {/* Add New Role */}
          <Box>
            <Typography variant="subtitle2" gutterBottom>
              Add New Role
            </Typography>
            <Box sx={{ display: "flex", gap: 1 }}>
              <FormControl fullWidth size="small">
                <InputLabel>Role</InputLabel>
                <Select
                  value={newRole}
                  label="Role"
                  onChange={(e) => setNewRole(e.target.value)}
                  disabled={selectableRoles.length === 0}
                >
                  {selectableRoles.map((role) => (
                    <MenuItem key={role} value={role}>
                      {role}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Button
                variant="contained"
                onClick={handleAddRole}
                disabled={!newRole}
              >
                Add
              </Button>
            </Box>
            {rolesToAdd.length > 0 && (
              <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap", mt: 1 }}>
                {rolesToAdd.map((role) => (
                  <Chip
                    key={role}
                    label={role}
                    color="success"
                    onDelete={() => handleRemoveFromAdding(role)}
                  />
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
          disabled={rolesToAdd.length === 0 && rolesToRemove.length === 0}
        >
          Save Changes
        </Button>
      </DialogActions>
    </Dialog>
  );
};
