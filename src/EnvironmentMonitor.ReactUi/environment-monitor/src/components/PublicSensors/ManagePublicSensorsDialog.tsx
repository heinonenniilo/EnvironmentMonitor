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

const measurementTypeOptions = [
  { label: "Temperature", value: MeasurementTypes.Temperature },
  { label: "Humidity", value: MeasurementTypes.Humidity },
  { label: "Light", value: MeasurementTypes.Light },
  { label: "Motion", value: MeasurementTypes.Motion },
];

export const PublicSensorDialog: React.FC<PublicSensorDialogProps> = ({
  open,
  onClose,
  onSave,
  allSensors,
  devices,
  editingSensor,
}) => {
  const isEditing = !!editingSensor;

  const [formName, setFormName] = useState("");
  const [formSensorIdentifier, setFormSensorIdentifier] = useState("");
  const [formTypeId, setFormTypeId] = useState<number | undefined>(undefined);
  const [formIsActive, setFormIsActive] = useState(true);
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

  useEffect(() => {
    if (open) {
      if (editingSensor) {
        setFormName(editingSensor.name ?? "");
        setFormSensorIdentifier(editingSensor.parentIdentifier ?? "");
        setFormTypeId(editingSensor.measurementType);
        setFormIsActive(editingSensor.active ?? true);
        // Resolve device from sensor's parentIdentifier
        const sourceSensor = allSensors.find(
          (s) => s.identifier === editingSensor.parentIdentifier,
        );
        setFormDeviceIdentifier(sourceSensor?.parentIdentifier ?? null);
      } else {
        setFormName("");
        setFormSensorIdentifier("");
        setFormTypeId(undefined);
        setFormIsActive(true);
        setFormDeviceIdentifier(null);
      }
    }
  }, [open, editingSensor, allSensors]);

  const getDeviceDisplayName = (device: Device): string => {
    return device.displayName ?? device.name;
  };

  const handleSave = () => {
    if (!formName.trim() || !(formSensorIdentifier ?? "").trim()) return;

    onSave({
      identifier: editingSensor?.identifier,
      name: formName.trim(),
      sensorIdentifier: (formSensorIdentifier ?? "").trim(),
      typeId: formTypeId,
      active: formIsActive,
    });
  };

  const isFormValid =
    formName.trim() !== "" && (formSensorIdentifier ?? "").trim() !== "";

  const isFormDirty =
    !editingSensor ||
    formName.trim() !== editingSensor.name ||
    formSensorIdentifier !== editingSensor.parentIdentifier ||
    formTypeId !== editingSensor.measurementType ||
    formIsActive !== (editingSensor.active ?? true);

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
            value={formName}
            onChange={(e) => setFormName(e.target.value)}
            fullWidth
            required
            size="small"
            error={formName.length > 0 && !formName.trim()}
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
              setFormSensorIdentifier("");
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
              allSensors.find((s) => s.identifier === formSensorIdentifier) ??
              null
            }
            onChange={(_e, value) => {
              setFormSensorIdentifier(value?.identifier ?? "");
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
              measurementTypeOptions.find((o) => o.value === formTypeId) ?? null
            }
            onChange={(_e, value) => {
              setFormTypeId(value?.value);
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
                checked={formIsActive}
                onChange={(e) => setFormIsActive(e.target.checked)}
                size="small"
              />
            }
            label="Active"
          />
          {formSensorIdentifier && (
            <Box>
              <Typography variant="subtitle2" gutterBottom>
                Summary
              </Typography>
              <Typography variant="body2">
                Name: <strong>{formName || "-"}</strong>
              </Typography>
              <Typography variant="body2">
                Source Sensor:{" "}
                <strong>
                  {allSensors.find((s) => s.identifier === formSensorIdentifier)
                    ?.name ?? formSensorIdentifier}
                </strong>
              </Typography>
              <Typography variant="body2">
                Type:{" "}
                <strong>
                  {measurementTypeOptions.find((o) => o.value === formTypeId)
                    ?.label ?? "-"}
                </strong>
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
