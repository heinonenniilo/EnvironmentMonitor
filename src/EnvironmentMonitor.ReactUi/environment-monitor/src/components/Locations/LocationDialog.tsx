import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  TextField,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { useEffect, useState } from "react";

export interface LocationDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (name: string) => void;
}

export const LocationDialog: React.FC<LocationDialogProps> = ({
  open,
  onClose,
  onSave,
}) => {
  const [name, setName] = useState("");

  useEffect(() => {
    if (open) {
      setName("");
    }
  }, [open]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Box>Add Location</Box>
        <IconButton onClick={onClose} size="small">
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <TextField
          autoFocus
          margin="dense"
          label="Name"
          value={name}
          onChange={(event) => setName(event.target.value)}
          fullWidth
          required
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={() => onSave(name.trim())}
          disabled={!name.trim()}
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};
