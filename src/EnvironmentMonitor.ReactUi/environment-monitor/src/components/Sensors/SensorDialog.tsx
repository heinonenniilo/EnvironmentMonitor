import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  IconButton,
  TextField,
  FormControlLabel,
  Checkbox,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { useState, useEffect } from "react";
import type { AddOrUpdateSensor } from "../../models/addOrUpdateSensor";
import type { SensorInfo } from "../../models/sensor";

export interface SensorDialogProps {
  open: boolean;
  deviceIdentifier: string;
  nextSensorId?: number;
  sensor?: SensorInfo | null;
  onClose: () => void;
  onSave?: (model: AddOrUpdateSensor) => void;
}

export const SensorDialog: React.FC<SensorDialogProps> = ({
  open,
  deviceIdentifier,
  nextSensorId,
  sensor,
  onClose,
  onSave,
}) => {
  const isEditing = !!sensor;

  const [name, setName] = useState("");
  const [sensorId, setSensorId] = useState("");
  const [scaleMin, setScaleMin] = useState("");
  const [scaleMax, setScaleMax] = useState("");
  const [isActive, setIsActive] = useState(true);

  useEffect(() => {
    if (open) {
      if (sensor) {
        setName(sensor.name);
        setSensorId(sensor.sensorId?.toString() ?? "");
        setScaleMin(sensor.scaleMin?.toString() ?? "");
        setScaleMax(sensor.scaleMax?.toString() ?? "");
        setIsActive(sensor.active ?? true);
      } else {
        setName("");
        setSensorId(nextSensorId !== undefined ? nextSensorId.toString() : "");
        setScaleMin("");
        setScaleMax("");
        setIsActive(true);
      }
    }
  }, [open, sensor, nextSensorId]);

  const handleSave = () => {
    if (!onSave || !name.trim() || !deviceIdentifier) {
      return;
    }

    const model: AddOrUpdateSensor = {
      identifier: sensor?.identifier,
      deviceIdentifier: deviceIdentifier,
      name: name.trim(),
      sensorId: sensorId ? parseInt(sensorId) : undefined,
      scaleMin: scaleMin ? parseFloat(scaleMin) : undefined,
      scaleMax: scaleMax ? parseFloat(scaleMax) : undefined,
      active: isActive,
    };

    onSave(model);
    onClose();
  };

  const isSaveDisabled = () => {
    if (!name.trim()) {
      return true;
    }
    if (isEditing && sensor) {
      const nameChanged = name.trim() !== sensor.name;
      const sensorIdChanged =
        (sensorId || "") !== (sensor.sensorId?.toString() ?? "");
      const scaleMinChanged =
        (scaleMin || "") !== (sensor.scaleMin?.toString() ?? "");
      const scaleMaxChanged =
        (scaleMax || "") !== (sensor.scaleMax?.toString() ?? "");
      const isActiveChanged = isActive !== (sensor.active ?? true);
      return (
        !nameChanged &&
        !sensorIdChanged &&
        !scaleMinChanged &&
        !scaleMaxChanged &&
        !isActiveChanged
      );
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
        <Box>{isEditing ? "Edit Sensor" : "Add New Sensor"}</Box>
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
            label="Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            fullWidth
            required
            error={name.length > 0 && !name.trim()}
          />
          <TextField
            label="Sensor Id"
            type="number"
            value={sensorId}
            onChange={(e) => setSensorId(e.target.value)}
            fullWidth
          />
          <TextField
            label="Scale Min"
            type="number"
            value={scaleMin}
            onChange={(e) => setScaleMin(e.target.value)}
            fullWidth
          />
          <TextField
            label="Scale Max"
            type="number"
            value={scaleMax}
            onChange={(e) => setScaleMax(e.target.value)}
            fullWidth
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={isActive}
                onChange={(e) => setIsActive(e.target.checked)}
              />
            }
            label="Active"
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
