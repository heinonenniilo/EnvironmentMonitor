import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
} from "@mui/material";
import React, { useState } from "react";
import moment from "moment";
import { DesktopDatePicker } from "@mui/x-date-pickers/DesktopDatePicker";
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";

export interface MeasurementsLeftViewProps {
  onSearch: (from: string, to: string, sensorIds: number[]) => void;
  onSelectDevice: (deviceId: string) => void;
  toggleSensorSelection: (sensorId: number) => void;
  devices: Device[];
  sensors: Sensor[];
  selectedSensors: number[];
  selectedDevice: Device | undefined;
}

export const MeasurementsLeftView: React.FC<MeasurementsLeftViewProps> = ({
  onSearch,
  devices,
  sensors,
  onSelectDevice,
  toggleSensorSelection,
  selectedSensors,
  selectedDevice,
}) => {
  const [fromDate, setFromDate] = useState<moment.Moment>(
    moment().utc(true).add(-2, "day").startOf("day")
  );
  const [toDate, setToDate] = useState<moment.Moment>(
    moment().utc().endOf("day")
  );

  return (
    <Box
      display={"flex"}
      flexDirection={"column"}
      width={"100%"}
      justifyContent={"space-between"}
    >
      <Box mt={2}>
        <DesktopDatePicker
          label="From"
          format="DD.MM.YYYY"
          value={fromDate}
          onChange={(value) => {
            if (value) {
              setFromDate((value as moment.Moment).utc(true).startOf("day"));
            }
          }}
        ></DesktopDatePicker>
      </Box>

      <Box mt={2}>
        <DesktopDatePicker
          label="To"
          format="DD.MM.YYYY"
          value={toDate}
          onChange={(value) => {
            if (value) {
              setToDate((value as moment.Moment).utc(true).endOf("day"));
            }
          }}
        />
      </Box>
      <Box mt={2}>
        <FormControl fullWidth>
          <InputLabel id="device-select-label">Device</InputLabel>
          <Select
            labelId="device-select-label"
            id="device-select"
            value={
              selectedDevice?.deviceIdentifier ?? {
                id: 0,
                deviceIdentifier: "",
              }
            }
            label="Device"
          >
            {devices.map((y) => (
              <MenuItem
                value={y.deviceIdentifier}
                key={`device-${y.deviceIdentifier}`}
                onClick={() => {
                  onSelectDevice(y.deviceIdentifier);
                }}
              >
                {y.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>
      <Box mt={2}>
        <FormControl fullWidth>
          <InputLabel id="device-select-label">Sensor</InputLabel>
          <Select
            labelId="sensor-select-label"
            id="sensor-select"
            value={selectedSensors}
            multiple
            label="Sensor"
          >
            {sensors.map((y) => (
              <MenuItem
                value={y.id}
                key={`sensor-${y.id}`}
                onClick={() => {
                  toggleSensorSelection(y.id);
                }}
              >
                {y.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <Box mt={2}>
        <Button
          variant="outlined"
          onClick={() => {
            if (selectedSensors.length > 0) {
              onSearch(
                fromDate.toISOString(),
                toDate.toISOString(),
                selectedSensors
              );
            }
          }}
        >
          Search
        </Button>
      </Box>
    </Box>
  );
};
