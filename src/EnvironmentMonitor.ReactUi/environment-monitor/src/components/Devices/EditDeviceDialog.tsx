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
  const [model, setModel] = useState<UpdateDeviceDto | undefined>(undefined);

  const createModel = (device: DeviceInfo): UpdateDeviceDto => ({
    device: { ...device.device },
    communicationChannelId: device.communicationChannelId ?? 0,
    deviceIdentifier: device.deviceIdentifier,
  });

  useEffect(() => {
    if (!open || !device) {
      return;
    }

    setModel(createModel(device));
  }, [device, open]);

  const handleSave = () => {
    if (!model || !model.device.name.trim() || !model.deviceIdentifier?.trim()) {
      return;
    }

    onSave({
      ...model,
      device: { ...model.device, name: model.device.name.trim() },
      deviceIdentifier: model.deviceIdentifier.trim(),
    });
    onClose();
  };

  const hasChanges =
    !!device &&
    !!model &&
    (model.device.name.trim() !== device.device.name ||
      model.deviceIdentifier?.trim() !== device.deviceIdentifier ||
      model.device.visible !== device.device.visible ||
      model.communicationChannelId !== (device.communicationChannelId ?? 0));

  const isSaveDisabled =
    !model ||
    !model.device.name.trim() ||
    !model.deviceIdentifier?.trim() ||
    !hasChanges;

  const handleRevert = () => {
    if (!device) {
      return;
    }

    setModel(createModel(device));
  };

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
            value={model?.device.name ?? ""}
            onChange={(event) =>
              setModel((current) =>
                current
                  ? {
                      ...current,
                      device: { ...current.device, name: event.target.value },
                    }
                  : current,
              )
            }
            fullWidth
            required
          />
          <TextField
            label="Device identifier"
            value={model?.deviceIdentifier ?? ""}
            onChange={(event) =>
              setModel((current) =>
                current
                  ? {
                      ...current,
                      deviceIdentifier: event.target.value,
                    }
                  : current,
              )
            }
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
              value={model?.communicationChannelId ?? 0}
              onChange={(event) =>
                setModel((current) =>
                  current
                    ? {
                        ...current,
                        communicationChannelId: event.target.value as number,
                      }
                    : current,
                )
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
                checked={model?.device.visible ?? false}
                onChange={(event) =>
                  setModel((current) =>
                    current
                      ? {
                          ...current,
                          device: {
                            ...current.device,
                            visible: event.target.checked,
                          },
                        }
                      : current,
                  )
                }
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
        <Button onClick={handleRevert} color="inherit" disabled={!hasChanges}>
          Revert
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
