import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  MenuItem,
  TextField,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { useEffect, useMemo, useState } from "react";
import type { Device } from "../../models/device";
import type { Sensor } from "../../models/sensor";
import type { AddOrUpdateLocationSensor } from "../../models/addOrUpdateLocationSensor";
import {
  getAvailableMeasurementTypes,
  getMeasurementTypeDisplayName,
} from "../../utilities/measurementUtils";
import { MeasurementTypes } from "../../enums/measurementTypes";
import { getEntityTitle } from "../../utilities/entityUtils";

export interface LocationSensorDialogProps {
  open: boolean;
  locationIdentifier: string;
  sensor?: Sensor | null;
  availableSensors: Sensor[];
  allSensors: Sensor[];
  devices: Device[];
  onClose: () => void;
  onSave: (model: AddOrUpdateLocationSensor) => void;
}

export const LocationSensorDialog: React.FC<LocationSensorDialogProps> = ({
  open,
  locationIdentifier,
  sensor,
  availableSensors,
  allSensors,
  devices,
  onClose,
  onSave,
}) => {
  const isEditing = !!sensor;
  const [selectedDeviceIdentifier, setSelectedDeviceIdentifier] = useState("");
  const resolvedSensor = useMemo(() => {
    if (!sensor) {
      return null;
    }

    return (
      allSensors.find((item) => item.identifier === sensor.identifier) ?? sensor
    );
  }, [allSensors, sensor]);

  const selectableSensors = useMemo(() => {
    if (!resolvedSensor) {
      return availableSensors;
    }

    if (
      availableSensors.some(
        (item) => item.identifier === resolvedSensor.identifier,
      )
    ) {
      return availableSensors;
    }

    return [resolvedSensor, ...availableSensors];
  }, [availableSensors, resolvedSensor]);

  const filteredSensors = useMemo(() => {
    if (!selectedDeviceIdentifier) {
      return selectableSensors;
    }

    return selectableSensors.filter(
      (item) => item.parentIdentifier === selectedDeviceIdentifier,
    );
  }, [selectableSensors, selectedDeviceIdentifier]);

  const [sensorIdentifier, setSensorIdentifier] = useState("");
  const [name, setName] = useState("");
  const [typeId, setTypeId] = useState<string>("");

  useEffect(() => {
    if (!open) {
      return;
    }

    setSensorIdentifier(resolvedSensor?.identifier ?? "");
    setName(sensor?.name ?? "");
    setTypeId(sensor?.measurementType?.toString() ?? "");
    setSelectedDeviceIdentifier(resolvedSensor?.parentIdentifier ?? "");
  }, [open, resolvedSensor, sensor]);

  const isSaveDisabled = () => {
    if (!sensorIdentifier || !name.trim()) {
      return true;
    }

    if (!sensor) {
      return false;
    }

    return (
      sensorIdentifier === sensor.identifier &&
      name.trim() === sensor.name &&
      typeId === (sensor.measurementType?.toString() ?? "")
    );
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
        <Box>{isEditing ? "Edit Location Sensor" : "Add Location Sensor"}</Box>
        <IconButton onClick={onClose} size="small">
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <Box display="flex" flexDirection="column" gap={2} mt={1}>
          <TextField
            select
            label="Device Filter"
            value={selectedDeviceIdentifier}
            onChange={(event) => {
              const nextDeviceIdentifier = event.target.value;
              setSelectedDeviceIdentifier(nextDeviceIdentifier);

              if (
                sensorIdentifier &&
                nextDeviceIdentifier &&
                !selectableSensors.some(
                  (item) =>
                    item.identifier === sensorIdentifier &&
                    item.parentIdentifier === nextDeviceIdentifier,
                )
              ) {
                setSensorIdentifier("");
              }
            }}
            disabled={isEditing}
            fullWidth
          >
            <MenuItem value="">All devices</MenuItem>
            {devices.map((device) => (
              <MenuItem key={device.identifier} value={device.identifier}>
                {getEntityTitle(device)}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Sensor"
            value={sensorIdentifier}
            onChange={(event) => setSensorIdentifier(event.target.value)}
            disabled={isEditing}
            fullWidth
          >
            {filteredSensors.map((item) => {
              const device = devices.find(
                (candidate) => candidate.identifier === item.parentIdentifier,
              );

              return (
                <MenuItem key={item.identifier} value={item.identifier}>
                  {item.name} -{" "}
                  {device ? getEntityTitle(device) : "Unknown device"}
                </MenuItem>
              );
            })}
          </TextField>
          <TextField
            label="Name"
            value={name}
            onChange={(event) => setName(event.target.value)}
            fullWidth
            required
          />
          <TextField
            select
            label="Measurement Type"
            value={typeId}
            onChange={(event) => setTypeId(event.target.value)}
            fullWidth
          >
            <MenuItem value="">Use sensor default</MenuItem>
            {getAvailableMeasurementTypes().map((value) => (
              <MenuItem key={value} value={value}>
                {getMeasurementTypeDisplayName(value as MeasurementTypes, true)}
              </MenuItem>
            ))}
          </TextField>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Button
          variant="contained"
          disabled={isSaveDisabled()}
          onClick={() => {
            onSave({
              locationIdentifier,
              sensorIdentifier,
              name: name.trim(),
              typeId: typeId ? Number(typeId) : undefined,
            });
          }}
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};
