import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useMemo } from "react";
import {
  getDashboardTimeRange,
  getDevices,
  getSensors,
  setDashboardTimeRange,
} from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { TimeRangeSelectorComponent } from "../components/TimeRangeSelectorComponent";
import { DashboardDeviceGraph } from "../components/DashboardDeviceGraph";
import { type Sensor } from "../models/sensor";
import { type Device } from "../models/device";

interface DeviceDashboardModel {
  sensors: Sensor[];
  device: Device;
}

export const DashboardView: React.FC = () => {
  const dispatch = useDispatch();

  const sensors = useSelector(getSensors);
  const devices = useSelector(getDevices);

  const timeRange = useSelector(getDashboardTimeRange);

  const handleTimeRangeChange = (selection: number) => {
    dispatch(setDashboardTimeRange(selection));
  };

  const measurementsModel: DeviceDashboardModel[] = useMemo(() => {
    return devices.map((device) => {
      const deviceSensors = sensors.filter((s) => s.deviceId === device.id);

      return { device, sensors: deviceSensors };
    });
  }, [devices, sensors]);

  const list = measurementsModel
    .slice()
    .sort((a, b) => {
      const locA = a.device.locationId ?? 0;
      const locB = b.device.locationId ?? 0;
      return locA - locB;
    })
    .map(({ device, sensors }) => {
      return (
        <DashboardDeviceGraph
          device={device}
          model={undefined}
          sensors={sensors}
          key={device.id}
          timeRange={timeRange}
        />
      );
    });

  return (
    <AppContentWrapper
      title="Dashboard - Devices"
      titleComponent={
        <TimeRangeSelectorComponent
          timeRange={timeRange}
          onSelectTimeRange={handleTimeRangeChange}
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
