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
  FormControl,
  InputLabel,
  MenuItem,
  Select,
} from "@mui/material";
import type { SelectChangeEvent } from "@mui/material";
import { Close } from "@mui/icons-material";
import { useState, useEffect } from "react";
import type { AddOrUpdateSensor } from "../../models/addOrUpdateSensor";
import type { SensorInfo } from "../../models/sensor";
import { AggregationTypes } from "../../enums/aggregationTypes";
import type { DeviceInfo } from "../../models/deviceInfo";

export interface SensorDialogProps {
  open: boolean;
  device: DeviceInfo | undefined;
  nextSensorId?: number;
  sensor?: SensorInfo | null;
  onClose: () => void;
  onSave?: (model: AddOrUpdateSensor) => void;
}

export const SensorDialog: React.FC<SensorDialogProps> = ({
  open,
  device,
  nextSensorId,
  sensor,
  onClose,
  onSave,
}) => {
  const isEditing = !!sensor;
  const deviceIdentifier = device?.device.identifier ?? "";
  const [name, setName] = useState("");
  const [sensorId, setSensorId] = useState("");
  const [scaleMin, setScaleMin] = useState("");
  const [scaleMax, setScaleMax] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [isVirtual, setIsVirtual] = useState(false);
  const [aggregationType, setAggregationType] = useState<AggregationTypes>(
    AggregationTypes.Min,
  );

  useEffect(() => {
    if (open) {
      if (sensor) {
        setName(sensor.name);
        setSensorId(sensor.sensorId?.toString() ?? "");
        setScaleMin(sensor.scaleMin?.toString() ?? "");
        setScaleMax(sensor.scaleMax?.toString() ?? "");
        setIsActive(sensor.active ?? true);
        setIsVirtual(sensor.isVirtual ?? false);
        setAggregationType(sensor.aggregationType ?? AggregationTypes.Min);
      } else {
        setName("");
        setSensorId(nextSensorId !== undefined ? nextSensorId.toString() : "");
        setScaleMin("");
        setScaleMax("");
        setIsActive(true);
        setIsVirtual(device?.isVirtual ?? false);
        setAggregationType(AggregationTypes.Min);
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, sensor, nextSensorId]);

  const handleAggregationTypeChange = (event: SelectChangeEvent<number>) => {
    setAggregationType(Number(event.target.value) as AggregationTypes);
  };

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
      isVirtual,
      aggregationType: isVirtual ? aggregationType : undefined,
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
      const isVirtualChanged = isVirtual !== (sensor.isVirtual ?? false);
      const aggregationTypeChanged =
        aggregationType !== (sensor.aggregationType ?? AggregationTypes.Min);
      return (
        !nameChanged &&
        !sensorIdChanged &&
        !scaleMinChanged &&
        !scaleMaxChanged &&
        !isActiveChanged &&
        !isVirtualChanged &&
        (!isVirtual || !aggregationTypeChanged)
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
          <FormControlLabel
            control={
              <Checkbox
                disabled={device?.isVirtual ?? false}
                checked={isVirtual}
                onChange={(e) => setIsVirtual(e.target.checked)}
              />
            }
            label="Virtual"
          />
          {isVirtual && (
            <FormControl fullWidth>
              <InputLabel id="aggregation-type-label">
                Aggregation Type
              </InputLabel>
              <Select
                labelId="aggregation-type-label"
                label="Aggregation Type"
                value={aggregationType}
                onChange={handleAggregationTypeChange}
              >
                <MenuItem value={AggregationTypes.Min}>MIN</MenuItem>
                <MenuItem value={AggregationTypes.Max}>MAX</MenuItem>
              </Select>
            </FormControl>
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
          disabled={isSaveDisabled()}
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};
