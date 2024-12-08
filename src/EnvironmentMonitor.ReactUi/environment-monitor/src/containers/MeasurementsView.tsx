import { useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { useApiHook } from "../hooks/apiHook";
import { MeasurementsLeftView } from "../components/MeasurementsLeftView";
import {
  getDashboardTimeRange,
  getDevices,
  getSensors,
} from "../reducers/measurementReducer";
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

  const [timeFrom, setTimeFrom] = useState<moment.Moment | undefined>(
    undefined
  );

  const devices = useSelector(getDevices);
  const sensors = useSelector(getSensors);
  const dashboardTimeRange = useSelector(getDashboardTimeRange);
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

        const fromDate = moment()
          .utc(true)
          .add(-1 * dashboardTimeRange, "hour")
          .startOf("day");

        setTimeFrom(fromDate);
        measurementApiHook
          .getMeasurementsBySensor(sensorIds, fromDate, undefined)
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [deviceId, devices]);

  return (
    <AppContentWrapper
      titleParts={[{ text: "Measurements" }]}
      isLoading={isLoading}
      leftMenu={
        <MeasurementsLeftView
          onSearch={(
            from: moment.Moment,
            to: moment.Moment | undefined,
            sensorIds: number[]
          ) => {
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
          timeFrom={timeFrom}
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
        }}
      >
        <MultiSensorGraph
          sensors={selectedSensors}
          model={measurementsModel}
          device={selectedDevice}
          key={"graph_01"}
          minHeight={500}
        />
      </Box>
    </AppContentWrapper>
  );
};
