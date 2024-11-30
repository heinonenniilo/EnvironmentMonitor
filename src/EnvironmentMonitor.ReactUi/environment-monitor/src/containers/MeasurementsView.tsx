import { useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useState } from "react";
import { useApiHook } from "../hooks/apiHook";
import { MeasurementsLeftView } from "../components/MeasurementsLeftView";
import { getDevices, getSensors } from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { Device } from "../models/device";
import { MeasurementsViewModel } from "../models/measurementsBySensor";
import { MultiSensorGraph } from "../components/MultiSensorGraph";
import { Sensor } from "../models/sensor";

export const MeasurementsView: React.FC = () => {
  const measurementApiHook = useApiHook().measureHook;
  const [isLoading, setIsLoading] = useState(false);

  const [selectedDevice, setSelectedDevice] = useState<Device | undefined>(
    undefined
  );

  const [measurementsModel, setMeasurementsModel] = useState<
    MeasurementsViewModel | undefined
  >(undefined);

  const devices = useSelector(getDevices);
  const sensors = useSelector(getSensors);

  const [selectedSensors, setSelectedSensors] = useState<Sensor[]>([]);

  return (
    <AppContentWrapper
      titleParts={[{ text: "Measurements" }]}
      isLoading={isLoading}
      leftMenu={
        <MeasurementsLeftView
          onSearch={(from: Date, to: Date, sensorIds: number[]) => {
            setIsLoading(true);
            measurementApiHook
              .getMeasurementsBySensor(sensorIds, from, to)
              .then((res) => {
                setSelectedSensors(
                  sensors.filter((sensor) =>
                    sensorIds.some((s) => sensor.id === s)
                  )
                );
                setMeasurementsModel(res);
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
        <MultiSensorGraph
          sensors={selectedSensors}
          model={measurementsModel}
          device={selectedDevice}
        />
      </Box>
    </AppContentWrapper>
  );
};
