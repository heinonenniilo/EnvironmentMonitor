import { useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { useApiHook } from "../hooks/apiHook";
import { MeasurementsLeftView } from "../components/MeasurementsLeftView";
import { getDevices, getSensors } from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { Device } from "../models/device";
import { MeasurementsViewModel } from "../models/measurementsBySensor";
import { MultiSensorGraph } from "../components/MultiSensorGraph";
import { Sensor } from "../models/sensor";
import { useParams } from "react-router";
import moment from "moment";

export const MeasurementsView: React.FC = () => {
  const measurementApiHook = useApiHook().measureHook;
  const [isLoading, setIsLoading] = useState(false);

  const { deviceId } = useParams<{ deviceId?: string }>();

  const [selectedDevice, setSelectedDevice] = useState<Device | undefined>(
    undefined
  );

  const [measurementsModel, setMeasurementsModel] = useState<
    MeasurementsViewModel | undefined
  >(undefined);

  const devices = useSelector(getDevices);
  const sensors = useSelector(getSensors);
  const [selectedSensors, setSelectedSensors] = useState<Sensor[]>([]);

  const toggleSensorSelection = (sensorId: number) => {
    if (selectedSensors.some((s) => s.id === sensorId)) {
      setSelectedSensors([...selectedSensors.filter((s) => s.id !== sensorId)]);
    } else {
      const t = [...selectedSensors];
      const matchingSensor = sensors.find((s) => s.id === sensorId);
      if (matchingSensor) {
        t.push(matchingSensor);
        setSelectedSensors(t);
      }
    }
  };

  useEffect(() => {
    if (deviceId !== undefined && devices.length > 0) {
      const matchingDevice = devices.find(
        (d) => d.deviceIdentifier === deviceId
      );
      if (matchingDevice) {
        setSelectedDevice(matchingDevice);
        const sensorIds = sensors
          .filter((s) => s.deviceId === matchingDevice.id)
          .map((s) => s.id);
        setIsLoading(true);
        measurementApiHook
          .getMeasurementsBySensor(
            sensorIds,
            moment().utc(true).add(-2, "day").startOf("day").toDate(),
            undefined
          )
          .then((res) => {
            setSelectedSensors(
              sensors.filter((sensor) => sensorIds.some((s) => sensor.id === s))
            );
            setMeasurementsModel(res);
          })
          .finally(() => {
            setIsLoading(false);
          });
      }
    }
  }, [deviceId, devices]);

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
          toggleSensorSelection={toggleSensorSelection}
          selectedDevice={selectedDevice}
          selectedSensors={selectedSensors.map((s) => s.id)}
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
          key={"graph_01"}
        />
      </Box>
    </AppContentWrapper>
  );
};
