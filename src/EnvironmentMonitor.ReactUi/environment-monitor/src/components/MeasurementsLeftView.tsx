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
  onSearch: (from: Date, to: Date, sensorId: number) => void;
  getSensors: (deviceId: string) => void;
  devices: Device[];
  sensors: Sensor[];
}

export const MeasurementsLeftView: React.FC<MeasurementsLeftViewProps> = ({
  onSearch,
  devices,
  sensors,
  getSensors,
}) => {
  const [fromDate, setFromDate] = useState<moment.Moment>(
    moment().utc(true).add(-2, "day").startOf("day")
  );
  const [toDate, setToDate] = useState<moment.Moment>(
    moment().utc().endOf("day")
  );

  const [selectedSensor, setSelectedSensor] = useState<Sensor | undefined>(
    undefined
  );

  const [selectedDevice, setSelectedDevice] = useState<Device>({
    id: 0,
    deviceIdentifier: "",
    name: "",
  });

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
                  const device = devices.find((d) => d.id === y.id);
                  if (device) {
                    setSelectedDevice(device);
                    getSensors(device.deviceIdentifier);
                  }
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
            value={
              selectedSensor?.id ?? {
                id: 0,
              }
            }
            label="Device"
          >
            {sensors.map((y) => (
              <MenuItem
                value={y.id}
                key={`sensor-${y.id}`}
                onClick={() => {
                  const sensor = sensors.find((d) => d.id === y.id);
                  if (sensor) {
                    setSelectedSensor(sensor);
                  }
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
            if (selectedSensor && selectedSensor.id) {
              onSearch(fromDate.toDate(), toDate.toDate(), selectedSensor.id);
            }
          }}
        >
          Search
        </Button>
      </Box>
    </Box>
  );
};
