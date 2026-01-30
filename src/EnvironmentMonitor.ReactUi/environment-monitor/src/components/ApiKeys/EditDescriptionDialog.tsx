import React, { useState, useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
} from "@mui/material";
import type { ApiKeyDto } from "../../models/apiKey";

interface EditDescriptionDialogProps {
  open: boolean;
  apiKey: ApiKeyDto | null;
  onClose: () => void;
  onSave: (apiKey: ApiKeyDto, newDescription: string) => void;
  isLoading?: boolean;
}

export const EditDescriptionDialog: React.FC<EditDescriptionDialogProps> = ({
  open,
  apiKey,
  onClose,
  onSave,
  isLoading,
}) => {
  const [description, setDescription] = useState<string>("");

  useEffect(() => {
    if (apiKey) {
      setDescription(apiKey.description || "");
    }
  }, [apiKey]);

  const handleSave = () => {
    if (apiKey) {
      onSave(apiKey, description);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSave();
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Edit API Key Description</DialogTitle>
      <DialogContent>
        <TextField
          autoFocus
          margin="dense"
          label="Description"
          type="text"
          fullWidth
          variant="outlined"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          onKeyDown={handleKeyDown}
          disabled={isLoading}
          multiline
          rows={3}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={isLoading}>
          Cancel
        </Button>
        <Button onClick={handleSave} variant="contained" disabled={isLoading}>
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};
