import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  IconButton,
  TextField,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { type DeviceContact } from "../models/deviceContact";
import { useState, useEffect } from "react";

export interface DeviceContactDialogProps {
  open: boolean;
  contact: DeviceContact | null;
  deviceIdentifier: string;
  onClose: () => void;
  onSave?: (
    email: string,
    deviceIdentifier: string,
    identifier?: string
  ) => void;
}

export const DeviceContactDialog: React.FC<DeviceContactDialogProps> = ({
  open,
  contact,
  deviceIdentifier,
  onClose,
  onSave,
}) => {
  const [email, setEmail] = useState("");

  useEffect(() => {
    if (contact) {
      setEmail(contact.email);
    } else {
      setEmail("");
    }
  }, [contact, open]);

  const handleSave = () => {
    if (!onSave || !email.trim() || !deviceIdentifier) {
      return;
    }

    onSave(email.trim(), deviceIdentifier, contact?.identifier);
    onClose();
  };

  const isEmailValid = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const isSaveDisabled = () => {
    if (!email.trim() || !isEmailValid(email)) {
      return true;
    }

    // If editing, check if email has changed
    if (contact) {
      return email.trim() === contact.email;
    }

    return false;
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle
        sx={{
          display: "flex",
          flexDirection: "row",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Box>{contact ? "Edit Contact" : "Add New Contact"}</Box>
        <IconButton
          aria-label="close"
          onClick={onClose}
          sx={{
            color: (theme) => theme.palette.grey[500],
          }}
          size="small"
        >
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <Box display="flex" flexDirection="column" gap={2} mt={1}>
          <TextField
            label="Email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            fullWidth
            required
            error={email.trim().length > 0 && !isEmailValid(email)}
            helperText={
              email.trim().length > 0 && !isEmailValid(email)
                ? "Please enter a valid email address"
                : ""
            }
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Button
          onClick={handleSave}
          color="primary"
          variant="contained"
          disabled={isSaveDisabled()}
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};
