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
import type { AddOrUpdateDeviceDto } from "../../models/addOrUpdateDeviceDto";
import type { LocationModel } from "../../models/location";
import {
  CommunicationChannels,
  getCommunicationChannelDisplayName,
} from "../../enums/communicationChannels";

interface DeviceDialogModel {
  name: string;
  deviceIdentifier: string;
  visible: boolean;
  isVirtual: boolean;
  communicationChannelId: number;
  locationIdentifier?: string;
}

export interface EditDeviceDialogProps {
  open: boolean;
  onClose: () => void;
  device?: DeviceInfo;
  locations?: LocationModel[];
  onSave: (model: AddOrUpdateDeviceDto) => void;
}

export const EditDeviceDialog: React.FC<EditDeviceDialogProps> = (props) => {
  const { open, device, locations, onClose } = props;
  const [model, setModel] = useState<DeviceDialogModel | undefined>(undefined);

  const createEditModel = (device: DeviceInfo): DeviceDialogModel => ({
    name: device.device.name,
    communicationChannelId: device.communicationChannelId ?? 0,
    deviceIdentifier: device.deviceIdentifier,
    visible: device.device.visible,
    isVirtual: device.isVirtual,
  });

  const createAddModel = (): DeviceDialogModel => ({
    name: "",
    communicationChannelId: CommunicationChannels.IotHub,
    deviceIdentifier: "",
    visible: true,
    isVirtual: false,
    locationIdentifier: undefined,
  });

  useEffect(() => {
    if (!open) {
      return;
    }

    setModel(device ? createEditModel(device) : createAddModel());
  }, [device, open]);

  const handleSave = () => {
    if (!model || !model.name.trim() || !model.deviceIdentifier.trim()) {
      return;
    }

    const trimmedModel = {
      ...model,
      name: model.name.trim(),
      deviceIdentifier: model.deviceIdentifier.trim(),
    };

    if (device) {
      props.onSave({
        identifier: device.device.identifier,
        name: trimmedModel.name,
        visible: trimmedModel.visible,
        isVirtual: trimmedModel.isVirtual,
        communicationChannelId: trimmedModel.communicationChannelId,
        deviceIdentifier: trimmedModel.deviceIdentifier,
      });
    } else {
      props.onSave({
        ...trimmedModel,
        locationIdentifier: trimmedModel.locationIdentifier || undefined,
      });
    }

    onClose();
  };

  const hasEditChanges =
    !!device &&
    !!model &&
    (model.name.trim() !== device.device.name ||
      model.deviceIdentifier.trim() !== device.deviceIdentifier ||
      model.visible !== device.device.visible ||
      model.isVirtual !== device.isVirtual ||
      model.communicationChannelId !== (device.communicationChannelId ?? 0));

  const hasAddChanges =
    !device &&
    !!model &&
    (model.name.trim().length > 0 ||
      model.deviceIdentifier.trim().length > 0 ||
      model.visible !== true ||
      model.isVirtual !== false ||
      !!model.locationIdentifier ||
      model.communicationChannelId !== CommunicationChannels.IotHub);

  const hasChanges = hasEditChanges || hasAddChanges;

  const isSaveDisabled =
    !model ||
    !model.name.trim() ||
    !model.deviceIdentifier.trim() ||
    (!!device && !hasEditChanges);

  const handleRevert = () => {
    setModel(device ? createEditModel(device) : createAddModel());
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
        <Box>{device ? "Edit Device" : "Add Device"}</Box>
        <IconButton onClick={onClose} size="small" aria-label="close">
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <Box display="flex" flexDirection="column" gap={2} pt={1}>
          <TextField
            autoFocus
            label="Name"
            value={model?.name ?? ""}
            onChange={(event) =>
              setModel((current) =>
                current
                  ? {
                      ...current,
                      name: event.target.value,
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
                        communicationChannelId: Number(event.target.value),
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
                checked={model?.visible ?? false}
                onChange={(event) =>
                  setModel((current) =>
                    current
                      ? {
                          ...current,
                          visible: event.target.checked,
                        }
                      : current,
                  )
                }
              />
            }
            label="Visible"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={model?.isVirtual ?? false}
                onChange={(event) =>
                  setModel((current) =>
                    current
                      ? {
                          ...current,
                          isVirtual: event.target.checked,
                        }
                      : current,
                  )
                }
              />
            }
            label="Virtual"
          />
          {!device && (
            <FormControl fullWidth>
              <InputLabel id="device-location-label">Location</InputLabel>
              <Select
                labelId="device-location-label"
                label="Location"
                value={model?.locationIdentifier ?? ""}
                onChange={(event) =>
                  setModel((current) =>
                    current
                      ? {
                          ...current,
                          locationIdentifier:
                            event.target.value === ""
                              ? undefined
                              : event.target.value,
                        }
                      : current,
                  )
                }
              >
                <MenuItem value="">None</MenuItem>
                {(locations ?? []).map((location) => (
                  <MenuItem key={location.identifier} value={location.identifier}>
                    {location.displayName || location.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
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
