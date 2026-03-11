import {
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  IconButton,
  TextField,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { useEffect, useState } from "react";
import type { LocationModel } from "../../models/location";

export interface EditLocationDialogProps {
  location?: LocationModel;
  open: boolean;
  onClose: () => void;
  onSave: (model: LocationModel) => void;
}

export const EditLocationDialog: React.FC<EditLocationDialogProps> = ({
  location,
  open,
  onClose,
  onSave,
}) => {
  const [name, setName] = useState("");
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    if (!open || !location) {
      return;
    }

    setName(location.name);
    setVisible(location.visible);
  }, [location, open]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Box>Edit Location</Box>
        <IconButton onClick={onClose} size="small">
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <Box display="flex" flexDirection="column" gap={1} pt={1}>
          <TextField
            autoFocus
            label="Name"
            value={name}
            onChange={(event) => setName(event.target.value)}
            fullWidth
            required
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={visible}
                onChange={(event) => setVisible(event.target.checked)}
              />
            }
            label="Visible"
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={() =>
            location &&
            onSave({
              ...location,
              name: name.trim(),
              visible,
            })
          }
          disabled={!location || !name.trim()}
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};
