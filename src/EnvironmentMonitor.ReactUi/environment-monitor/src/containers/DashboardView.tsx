import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useMemo, useState } from "react";
import {
  getDashboardAutoScale,
  getDashboardTimeRange,
  getDevices,
  getSensors,
  setDashboardTimeRange,
  toggleAutoScale,
} from "../reducers/measurementReducer";
import { useApiHook } from "../hooks/apiHook";
import { Box } from "@mui/material";
import moment from "moment";
import { MeasurementsViewModel } from "../models/measurementsBySensor";
import { MultiSensorGraph } from "../components/MultiSensorGraph";
import { TimeSelections } from "../enums/timeSelections";
import { TimeRangeSelectorComponent } from "../components/TimeRangeSelectorComponent";

export const DashboardView: React.FC = () => {
  const measurementApiHook = useApiHook().measureHook;
  const dispatch = useDispatch();
  const [viewModel, setViewModel] = useState<MeasurementsViewModel | undefined>(
    undefined
  );
  const [isLoading, setIsLoading] = useState(false);

  const sensors = useSelector(getSensors);
  const devices = useSelector(getDevices);
  const autoScaleIds = useSelector(getDashboardAutoScale);

  const timeRange = useSelector(getDashboardTimeRange);

  const handleTimeRangeChange = (selection: TimeSelections) => {
    dispatch(setDashboardTimeRange(selection));
  };

  useEffect(() => {
    if (!sensors || sensors.length === 0) {
      return;
    }
    const momentStart = moment()
      .local(true)
      .add(-1 * timeRange, "hour")
      .utc(true);
    setIsLoading(true);
    measurementApiHook
      .getMeasurementsBySensor(
        sensors.map((x) => x.id),
        momentStart,
        undefined
      )
      .then((res) => {
        setViewModel(res);
      })
      .catch((er) => {
        console.error(er);
        setViewModel(undefined);
      })
      .finally(() => {
        setIsLoading(false);
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeRange]);

  const memoizedMeasurements = useMemo(() => {
    return devices.map((device) => {
      const deviceSensors = sensors.filter((s) => s.deviceId === device.id);
      const measurementsModel: MeasurementsViewModel | undefined = viewModel
        ? {
            measurements: viewModel.measurements.filter((m) =>
              deviceSensors.some((s) => s.id === m.sensorId)
            ),
          }
        : undefined;

      return { device, deviceSensors, measurementsModel };
    });
  }, [devices, sensors, viewModel]);

  return (
    <AppContentWrapper
      titleParts={[{ text: "Dashboard" }]}
      isLoading={isLoading}
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
          {memoizedMeasurements.map(
            ({ device, deviceSensors, measurementsModel }) => {
              return (
                <Box
                  key={device.id}
                  sx={{
                    border: "1px solid #ccc",
                    borderRadius: 2,
                    padding: 1,
                    boxSizing: "border-box",
                    backgroundColor: "#f9f9f9",
                    height: "100%",
                    display: "flex",
                    flexDirection: "column",
                    justifyContent: "center",
                    alignItems: "center",
                  }}
                >
                  <MultiSensorGraph
                    sensors={deviceSensors}
                    devices={[device]}
                    model={measurementsModel}
                    minHeight={400}
                    titleAsLink
                    useAutoScale={autoScaleIds.includes(device.id)}
                    onSetAutoScale={(state) => {
                      dispatch(toggleAutoScale({ deviceId: device.id, state }));
                    }}
                  />
                </Box>
              );
            }
          )}
        </Box>
      </Box>
    </AppContentWrapper>
  );
};
