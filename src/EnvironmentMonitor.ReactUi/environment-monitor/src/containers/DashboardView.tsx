import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useMemo, useState } from "react";
import {
  getDashboardTimeRange,
  getDevices,
  getSensors,
  setDashboardTimeRange,
} from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { TimeRangeSelectorComponent } from "../components/TimeRangeSelectorComponent";
import { DashboardDeviceGraph } from "../components/Dashboard/DashboardDeviceGraph";
import { type Sensor } from "../models/sensor";
import { type Device } from "../models/device";
import { stringSort } from "../utilities/stringUtils";
import { DashboardLeftMenu } from "../components/Dashboard/DashboardLeftMenu";
import { MeasurementTypes } from "../enums/measurementTypes";

interface DeviceDashboardModel {
  sensors: Sensor[];
  device: Device;
}

// Get all available measurement types (excluding Undefined and Online)
const getAvailableMeasurementTypes = () => {
  return Object.keys(MeasurementTypes)
    .filter(
      (key) =>
        !isNaN(Number(MeasurementTypes[key as keyof typeof MeasurementTypes]))
    )
    .map((key) =>
      Number(MeasurementTypes[key as keyof typeof MeasurementTypes])
    )
    .filter(
      (value) =>
        value !== MeasurementTypes.Undefined &&
        value !== MeasurementTypes.Online
    );
};

export const DashboardView: React.FC = () => {
  const dispatch = useDispatch();
  const [selectedMeasurementTypes, setSelectedMeasurementTypes] = useState<
    number[]
  >(getAvailableMeasurementTypes());

  const sensors = useSelector(getSensors);
  const devices = useSelector(getDevices);

  const timeRange = useSelector(getDashboardTimeRange);

  const handleTimeRangeChange = (selection: number) => {
    setTimeout(() => {
      dispatch(setDashboardTimeRange(selection));
    }, 10);
  };

  const measurementsModel: DeviceDashboardModel[] = useMemo(() => {
    return [...devices]
      .sort((a, b) => stringSort(a.displayName, b.displayName))
      .map((device) => {
        const deviceSensors = sensors.filter(
          (s) => s.parentIdentifier === device.identifier
        );

        return { device, sensors: deviceSensors };
      });
  }, [devices, sensors]);

  const list = measurementsModel.map(({ device, sensors }) => {
    return (
      <DashboardDeviceGraph
        device={device}
        model={undefined}
        sensors={sensors}
        key={device.identifier}
        timeRange={timeRange}
        autoFetch
        measurementTypes={
          selectedMeasurementTypes.length > 0
            ? selectedMeasurementTypes
            : undefined
        }
      />
    );
  });

  const handleMeasurementTypesChange = (measurementTypes: number[]) => {
    setSelectedMeasurementTypes(measurementTypes);
  };

  return (
    <AppContentWrapper
      title="Dashboard - Devices"
      titleComponent={
        <TimeRangeSelectorComponent
          timeRange={timeRange}
          onSelectTimeRange={handleTimeRangeChange}
        />
      }
      leftMenu={
        <DashboardLeftMenu
          onMeasurementTypesChange={handleMeasurementTypesChange}
        />
      }
    >
      <Box
        sx={{
          display: "flex",
          gap: 1, // Space between grid items
          flexGrow: 1,
          height: "100%",
          flexDirection: "column",
        }}
      >
        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: {
              xs: "1fr", // Single column for extra-small screens
              lg: "1fr 1fr", // Two columns for large screens
            },
            gap: 1, // Space between grid items
            padding: 1, // Padding around the grid container
            flexGrow: 1,
            height: "100%",
          }}
        >
          {list}
        </Box>
      </Box>
    </AppContentWrapper>
  );
};
