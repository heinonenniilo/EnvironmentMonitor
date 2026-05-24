import {
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { useEffect, useState } from "react";
import type { DeviceInfo } from "../../models/deviceInfo";
import type { UpdateDeviceDto } from "../../models/updateDeviceDto";
import {
  CommunicationChannels,
  getCommunicationChannelDisplayName,
} from "../../enums/communicationChannels";

export interface EditDeviceDialogProps {
  open: boolean;
  device?: DeviceInfo;
  onClose: () => void;
  onSave: (model: UpdateDeviceDto) => void;
}

export const EditDeviceDialog: React.FC<EditDeviceDialogProps> = ({
  open,
  device,
  onClose,
  onSave,
}) => {
  const [name, setName] = useState("");
  const [deviceIdentifier, setDeviceIdentifier] = useState("");
  const [visible, setVisible] = useState(false);
  const [communicationChannelId, setCommunicationChannelId] = useState(0);

  useEffect(() => {
    if (!open || !device) {
      return;
    }

    setName(device.device.name);
    setDeviceIdentifier(device.deviceIdentifier);
    setVisible(device.device.visible);
    setCommunicationChannelId(device.communicationChannelId ?? 0);
  }, [device, open]);

  const handleSave = () => {
    if (!device || !name.trim() || !deviceIdentifier.trim()) {
      return;
    }

    onSave({
      device: {
        ...device.device,
        name: name.trim(),
        visible,
      },
      communicationChannelId,
      deviceIdentifier: deviceIdentifier.trim(),
    });
    onClose();
  };

  const isSaveDisabled =
    !device ||
    !name.trim() ||
    !deviceIdentifier.trim() ||
    (name.trim() === device.device.name &&
      deviceIdentifier.trim() === device.deviceIdentifier &&
      visible === device.device.visible &&
      communicationChannelId === (device.communicationChannelId ?? 0));

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Box>Edit Device</Box>
        <IconButton onClick={onClose} size="small" aria-label="close">
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <Box display="flex" flexDirection="column" gap={2} pt={1}>
          <TextField
            autoFocus
            label="Name"
            value={name}
            onChange={(event) => setName(event.target.value)}
            fullWidth
            required
          />
          <TextField
            label="Device identifier"
            value={deviceIdentifier}
            onChange={(event) => setDeviceIdentifier(event.target.value)}
            fullWidth
            required
          />
          <FormControl fullWidth>
            <InputLabel id="device-communication-channel-label">
              Communication channel
            </InputLabel>
            <Select
              labelId="device-communication-channel-label"
              label="Communication channel"
              value={communicationChannelId}
              onChange={(event) =>
                setCommunicationChannelId(event.target.value as number)
              }
            >
              {Object.values(CommunicationChannels)
                .filter((value) => typeof value === "number")
                .map((value) => (
                  <MenuItem key={value} value={value}>
                    {getCommunicationChannelDisplayName(
                      value as CommunicationChannels,
                    )}
                  </MenuItem>
                ))}
            </Select>
          </FormControl>
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
          onClick={handleSave}
          disabled={isSaveDisabled}
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};
