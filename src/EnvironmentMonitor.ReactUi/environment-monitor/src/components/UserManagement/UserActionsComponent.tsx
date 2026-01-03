import React from "react";
import { Box, IconButton, Tooltip } from "@mui/material";
import { ArrowBack, Delete } from "@mui/icons-material";

export interface UserActionsComponentProps {
  onBack: () => void;
  onDelete: () => void;
}

export const UserActionsComponent: React.FC<UserActionsComponentProps> = ({
  onBack,
  onDelete,
}) => {
  return (
    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
      <Tooltip title="Back to Users">
        <IconButton onClick={onBack} size="medium">
          <ArrowBack />
        </IconButton>
      </Tooltip>
      <Tooltip title="Delete User">
        <IconButton onClick={onDelete} size="medium" color="error">
          <Delete />
        </IconButton>
      </Tooltip>
    </Box>
  );
};
