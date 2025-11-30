import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { useApiHook } from "../hooks/apiHook";
import { MeasurementsLeftView } from "../components/MeasurementsLeftView";
import {
  getDashboardTimeRange,
  getDeviceAutoScale,
  getDevices,
  getSensors,
  toggleAutoScale,
} from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { type Device } from "../models/device";
import { type MeasurementsViewModel } from "../models/measurementsBySensor";
import { MultiSensorGraph } from "../components/MultiSensorGraph";
import { type Sensor } from "../models/sensor";
import { useParams } from "react-router";
import moment from "moment";

export const MeasurementsView: React.FC = () => {
  const measurementApiHook = useApiHook().measureHook;
  const dispatch = useDispatch();
  const [isLoading, setIsLoading] = useState(false);
  const { deviceId } = useParams<{ deviceId?: string }>();
  const [selectedDevices, setSelectedDevices] = useState<Device[]>([]);

  const [measurementsModel, setMeasurementsModel] = useState<
    MeasurementsViewModel | undefined
  >(undefined);

  const [timeFrom, setTimeFrom] = useState<moment.Moment | undefined>(
    undefined
  );
  const [timeTo, setTimeTo] = useState<moment.Moment | undefined>(undefined);
  const devices = useSelector(getDevices);
  const sensors = useSelector(getSensors);
  const dashboardTimeRange = useSelector(getDashboardTimeRange);
  const autoScaleInUseForDevice = useSelector(
    getDeviceAutoScale(
      selectedDevices.length === 1 ? selectedDevices[0].identifier : ""
    )
  );
  const [selectedSensors, setSelectedSensors] = useState<Sensor[]>([]);

  const toggleSensorSelection = (sensorId: string) => {
    if (selectedSensors.some((s) => s.identifier === sensorId)) {
      setSelectedSensors([
        ...selectedSensors.filter((s) => s.identifier !== sensorId),
      ]);
    } else {
      const matchingSensor = sensors.find((s) => s.identifier === sensorId);
      if (matchingSensor) {
        setSelectedSensors([...selectedSensors, matchingSensor]);
      }
    }
  };

  const toggleDeviceSelection = (deviceId: string) => {
    const matchingDevice = devices.find((d) => d.identifier === deviceId);

    if (!selectedDevices.some((s) => s.identifier === deviceId)) {
      if (matchingDevice) {
        setSelectedDevices([...selectedDevices, matchingDevice]);
      }
    } else {
      setSelectedSensors(
        selectedSensors.filter(
          (s) => s.deviceIdentifier !== matchingDevice?.identifier
        )
      );
      setSelectedDevices(
        selectedDevices.filter((s) => s.identifier !== deviceId)
      );
    }
  };

  useEffect(() => {
    if (deviceId !== undefined && devices.length > 0) {
      const matchingDevice = devices.find((d) => d.identifier === deviceId);
      if (matchingDevice) {
        setSelectedDevices([matchingDevice]);
        const sensorIds = sensors
          .filter((s) => s.deviceIdentifier === matchingDevice.identifier)
          .map((s) => s.identifier);
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
              sensors.filter((sensor) =>
                sensorIds.some((s) => sensor.identifier === s)
              )
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

  const onSearch = (
    from: moment.Moment,
    to: moment.Moment | undefined,
    sensorIds: string[]
  ) => {
    setIsLoading(true);
    measurementApiHook
      .getMeasurementsBySensor(sensorIds, from, to)
      .then((res) => {
        setSelectedSensors(
          sensors.filter((sensor) =>
            sensorIds.some((s) => sensor.identifier === s)
          )
        );
        setMeasurementsModel(res);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  return (
    <AppContentWrapper
      title="Measurements"
      isLoading={isLoading}
      leftMenu={
        <MeasurementsLeftView
          onSearch={(
            from: moment.Moment,
            to: moment.Moment | undefined,
            sensorIds: string[]
          ) => {
            setTimeFrom(from);
            setTimeTo(to);
            onSearch(from, to, sensorIds);
          }}
          onSelectDevice={toggleDeviceSelection}
          toggleSensorSelection={toggleSensorSelection}
          selectedEntities={selectedDevices}
          selectedSensors={selectedSensors.map((s) => s.identifier)}
          entities={devices}
          sensors={
            selectedDevices
              ? sensors.filter(
                  (s) =>
                    selectedDevices &&
                    selectedDevices.some(
                      (d) => d.identifier === s.deviceIdentifier
                    )
                )
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
          model={
            measurementsModel
              ? {
                  measurements: measurementsModel.measurements.filter((m) =>
                    selectedSensors.some(
                      (s) => s.identifier === m.sensorIdentifier
                    )
                  ),
                }
              : undefined
          }
          entities={selectedDevices}
          key={"graph_01"}
          minHeight={500}
          useAutoScale={
            selectedDevices.length === 1 ? autoScaleInUseForDevice : undefined
          }
          onSetAutoScale={(state) => {
            if (selectedDevices.length === 1) {
              dispatch(
                toggleAutoScale({
                  deviceIdentifier: selectedDevices[0].identifier,
                  state: state,
                })
              );
            }
          }}
          onRefresh={() => {
            if (timeFrom) {
              onSearch(
                timeFrom,
                timeTo,
                selectedSensors.map((s) => s.identifier)
              );
            }
          }}
        />
      </Box>
    </AppContentWrapper>
  );
};
