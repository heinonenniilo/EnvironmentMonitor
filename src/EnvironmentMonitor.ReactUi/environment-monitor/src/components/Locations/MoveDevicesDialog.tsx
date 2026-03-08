import {
  Autocomplete,
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
import type { Device } from "../../models/device";
import { getEntityTitle } from "../../utilities/entityUtils";

export interface MoveDevicesDialogProps {
  open: boolean;
  locationName: string;
  devices: Device[];
  onClose: () => void;
  onSave: (deviceIdentifiers: string[]) => void;
}

export const MoveDevicesDialog: React.FC<MoveDevicesDialogProps> = ({
  open,
  locationName,
  devices,
  onClose,
  onSave,
}) => {
  const [selectedDevices, setSelectedDevices] = useState<Device[]>([]);

  useEffect(() => {
    if (open) {
      setSelectedDevices([]);
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
        <Box>Move Devices to {locationName}</Box>
        <IconButton onClick={onClose} size="small">
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <Autocomplete
          multiple
          options={devices}
          value={selectedDevices}
          onChange={(_event, value) => setSelectedDevices(value)}
          isOptionEqualToValue={(option, value) =>
            option.identifier === value.identifier
          }
          getOptionLabel={(option) => getEntityTitle(option)}
          renderInput={(params) => (
            <TextField
              {...params}
              margin="dense"
              label="Devices"
              placeholder="Select devices"
            />
          )}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Button
          variant="contained"
          disabled={selectedDevices.length === 0}
          onClick={() =>
            onSave(selectedDevices.map((device) => device.identifier))
          }
        >
          Move
        </Button>
      </DialogActions>
    </Dialog>
  );
};
