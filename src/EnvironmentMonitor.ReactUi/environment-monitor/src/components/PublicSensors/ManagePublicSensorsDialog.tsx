import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  IconButton,
  TextField,
  Typography,
  Autocomplete,
  FormControlLabel,
  Switch,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { useState, useEffect, useMemo } from "react";
import type { Sensor } from "../../models/sensor";
import type { Device } from "../../models/device";
import type { AddOrUpdatePublicSensorDto } from "../../models/managePublicSensorsRequest";
import { MeasurementTypes } from "../../enums/measurementTypes";

export interface PublicSensorDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (sensor: AddOrUpdatePublicSensorDto) => void;
  allSensors: Sensor[];
  devices: Device[];
  /** When set, the dialog is in edit mode and pre-filled with this sensor's data */
  editingSensor?: Sensor;
}

interface PublicSensorFormState {
  name: string;
  sensorIdentifier: string;
  typeId?: number;
  isActive: boolean;
  latitude: string;
  longitude: string;
}

const measurementTypeOptions = [
  { label: "Temperature", value: MeasurementTypes.Temperature },
  { label: "Humidity", value: MeasurementTypes.Humidity },
  { label: "Light", value: MeasurementTypes.Light },
  { label: "Motion", value: MeasurementTypes.Motion },
];

const defaultFormState: PublicSensorFormState = {
  name: "",
  sensorIdentifier: "",
  typeId: undefined,
  isActive: true,
  latitude: "",
  longitude: "",
};

export const PublicSensorDialog: React.FC<PublicSensorDialogProps> = ({
  open,
  onClose,
  onSave,
  allSensors,
  devices,
  editingSensor,
}) => {
  const isEditing = !!editingSensor;

  const [formState, setFormState] =
    useState<PublicSensorFormState>(defaultFormState);
  const [formDeviceIdentifier, setFormDeviceIdentifier] = useState<
    string | null
  >(null);

  // Sort devices by display name
  const sortedDevices = useMemo(() => {
    return [...devices].sort((a, b) =>
      (a.displayName ?? a.name).localeCompare(b.displayName ?? b.name),
    );
  }, [devices]);

  // Filter sensors based on selected device and sort by name
  const filteredSensors = useMemo(() => {
    const sensors = !formDeviceIdentifier
      ? allSensors
      : allSensors.filter((s) => s.parentIdentifier === formDeviceIdentifier);
    return [...sensors].sort((a, b) => a.name.localeCompare(b.name));
  }, [allSensors, formDeviceIdentifier]);

  const updateFormState = (updates: Partial<PublicSensorFormState>) => {
    setFormState((current) => ({ ...current, ...updates }));
  };

  useEffect(() => {
    if (open) {
      if (editingSensor) {
        setFormState({
          name: editingSensor.name ?? "",
          sensorIdentifier: editingSensor.parentIdentifier ?? "",
          typeId: editingSensor.measurementType,
          isActive: editingSensor.active ?? true,
          latitude:
            editingSensor.latitude !== undefined &&
            editingSensor.latitude !== null
              ? String(editingSensor.latitude)
              : "",
          longitude:
            editingSensor.longitude !== undefined &&
            editingSensor.longitude !== null
              ? String(editingSensor.longitude)
              : "",
        });
        // Resolve device from sensor's parentIdentifier
        const sourceSensor = allSensors.find(
          (s) => s.identifier === editingSensor.parentIdentifier,
        );
        setFormDeviceIdentifier(sourceSensor?.parentIdentifier ?? null);
      } else {
        setFormState(defaultFormState);
        setFormDeviceIdentifier(null);
      }
    }
  }, [open, editingSensor, allSensors]);

  const getDeviceDisplayName = (device: Device): string => {
    return device.displayName ?? device.name;
  };

  const handleSave = () => {
    if (!formState.name.trim() || !formState.sensorIdentifier.trim()) return;

    const latitude =
      formState.latitude.trim() === ""
        ? undefined
        : Number(formState.latitude.trim());
    const longitude =
      formState.longitude.trim() === ""
        ? undefined
        : Number(formState.longitude.trim());

    onSave({
      identifier: editingSensor?.identifier,
      name: formState.name.trim(),
      sensorIdentifier: formState.sensorIdentifier.trim(),
      typeId: formState.typeId,
      active: formState.isActive,
      latitude,
      longitude,
    });
  };

  const isLatitudeValid =
    formState.latitude.trim() === "" ||
    !Number.isNaN(Number(formState.latitude.trim()));
  const isLongitudeValid =
    formState.longitude.trim() === "" ||
    !Number.isNaN(Number(formState.longitude.trim()));

  const isFormValid =
    formState.name.trim() !== "" &&
    formState.sensorIdentifier.trim() !== "" &&
    isLatitudeValid &&
    isLongitudeValid;

  const isFormDirty =
    !editingSensor ||
    formState.name.trim() !== editingSensor.name ||
    formState.sensorIdentifier !== editingSensor.parentIdentifier ||
    formState.typeId !== editingSensor.measurementType ||
    formState.isActive !== (editingSensor.active ?? true) ||
    formState.latitude.trim() !==
      (editingSensor.latitude !== undefined && editingSensor.latitude !== null
        ? String(editingSensor.latitude)
        : "") ||
    formState.longitude.trim() !==
      (editingSensor.longitude !== undefined && editingSensor.longitude !== null
        ? String(editingSensor.longitude)
        : "");

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
        <Box>{isEditing ? "Edit Public Sensor" : "Add Public Sensor"}</Box>
        <IconButton
          aria-label="close"
          onClick={onClose}
          sx={{ color: (theme) => theme.palette.grey[500] }}
          size="small"
        >
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <Box display="flex" flexDirection="column" gap={2} mt={1}>
          <TextField
            label="Name"
            value={formState.name}
            onChange={(e) => updateFormState({ name: e.target.value })}
            fullWidth
            required
            size="small"
            error={formState.name.length > 0 && !formState.name.trim()}
          />
          <Autocomplete
            options={sortedDevices}
            getOptionLabel={(option) => getDeviceDisplayName(option)}
            value={
              devices.find((d) => d.identifier === formDeviceIdentifier) ?? null
            }
            onChange={(_e, value) => {
              setFormDeviceIdentifier(value?.identifier ?? null);
              // Clear sensor selection when device changes
              updateFormState({ sensorIdentifier: "" });
            }}
            renderInput={(params) => (
              <TextField {...params} label="Device (filter)" size="small" />
            )}
            size="small"
            fullWidth
          />
          <Autocomplete
            options={filteredSensors}
            getOptionLabel={(option) => `${option.name} (${option.identifier})`}
            value={
              allSensors.find((s) => s.identifier === formState.sensorIdentifier) ??
              null
            }
            onChange={(_e, value) => {
              updateFormState({ sensorIdentifier: value?.identifier ?? "" });
            }}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Source Sensor"
                required
                size="small"
              />
            )}
            size="small"
            fullWidth
          />
          <Autocomplete
            options={measurementTypeOptions}
            getOptionLabel={(option) => option.label}
            value={
              measurementTypeOptions.find((o) => o.value === formState.typeId) ??
              null
            }
            onChange={(_e, value) => {
              updateFormState({ typeId: value?.value });
            }}
            renderInput={(params) => (
              <TextField {...params} label="Measurement Type" size="small" />
            )}
            size="small"
            fullWidth
          />
          <FormControlLabel
            control={
              <Switch
                checked={formState.isActive}
                onChange={(e) => updateFormState({ isActive: e.target.checked })}
                size="small"
              />
            }
            label="Active"
          />
          <TextField
            label="Latitude"
            value={formState.latitude}
            onChange={(e) => updateFormState({ latitude: e.target.value })}
            fullWidth
            size="small"
            error={!isLatitudeValid}
            helperText={!isLatitudeValid ? "Latitude must be a number" : " "}
          />
          <TextField
            label="Longitude"
            value={formState.longitude}
            onChange={(e) => updateFormState({ longitude: e.target.value })}
            fullWidth
            size="small"
            error={!isLongitudeValid}
            helperText={!isLongitudeValid ? "Longitude must be a number" : " "}
          />
          {formState.sensorIdentifier && (
            <Box>
              <Typography variant="subtitle2" gutterBottom>
                Summary
              </Typography>
              <Typography variant="body2">
                Name: <strong>{formState.name || "-"}</strong>
              </Typography>
              <Typography variant="body2">
                Source Sensor:{" "}
                <strong>
                  {allSensors.find(
                    (s) => s.identifier === formState.sensorIdentifier,
                  )?.name ?? formState.sensorIdentifier}
                </strong>
              </Typography>
              <Typography variant="body2">
                Type:{" "}
                <strong>
                  {measurementTypeOptions.find(
                    (o) => o.value === formState.typeId,
                  )?.label ?? "-"}
                </strong>
              </Typography>
              <Typography variant="body2">
                Latitude: <strong>{formState.latitude.trim() || "-"}</strong>
              </Typography>
              <Typography variant="body2">
                Longitude: <strong>{formState.longitude.trim() || "-"}</strong>
              </Typography>
            </Box>
          )}
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
          disabled={!isFormValid || !isFormDirty}
        >
          {isEditing ? "Save" : "Add"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
