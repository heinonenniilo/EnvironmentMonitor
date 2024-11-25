import { useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useState } from "react";
import { useApiHook } from "../hooks/apiHook";
import { MeasurementsLeftView } from "../components/MeasurementsLeftView";
import { getDevices, getSensors } from "../reducers/measurementReducer";
import { Measurement } from "../models/measurement";
import { MeasurementGraph } from "../components/MeasurementGraph";
import { Box } from "@mui/material";
import { Sensor } from "../models/sensor";
import { Device } from "../models/device";

export const MeasurementsView: React.FC = () => {
  const measurementApiHook = useApiHook().measureHook;
  const [isLoading, setIsLoading] = useState(false);
  const [selectedSensor, setSelectedSensor] = useState<Sensor | undefined>(
    undefined
  );
  const [selectedDevice, setSelectedDevice] = useState<Device | undefined>(
    undefined
  );

  const [measurements, setMeasurements] = useState<Measurement[]>([]);

  const devices = useSelector(getDevices);
  const sensors = useSelector(getSensors);

  return (
    <AppContentWrapper
      titleParts={[{ text: "Measurements" }]}
      isLoading={isLoading}
      leftMenu={
        <MeasurementsLeftView
          onSearch={(from: Date, to: Date, sensorId: number) => {
            console.log(to);
            setIsLoading(true);

            const selectedSensor = sensors.find((s) => s.id === sensorId);
            setSelectedSensor(selectedSensor);
            measurementApiHook
              .getMeasurements([sensorId], from, to)
              .then((res) => {
                setMeasurements(res);
              })
              .finally(() => {
                setIsLoading(false);
              });
          }}
          onSelectDevice={(deviceId: string) => {
            const matchingDevice = devices.find(
              (d) => d.deviceIdentifier === deviceId
            );
            if (matchingDevice) {
              setSelectedDevice(matchingDevice);
            }
          }}
          devices={devices}
          sensors={
            selectedDevice
              ? sensors.filter((s) => s.deviceId === selectedDevice.id)
              : []
          }
        />
      }
    >
      <Box
        flexGrow={1}
        flex={1}
        width={"100%"}
        height={"100%"}
        sx={{
          display: "flex",
          flexDirection: "column",
          //flexGrow: {
          //  xs: 1,
          //},
          //columnGap: {
          //  xl: 3,
          //},
        }}
      >
        <MeasurementGraph sensor={selectedSensor} measurements={measurements} />
      </Box>
    </AppContentWrapper>
  );
};
