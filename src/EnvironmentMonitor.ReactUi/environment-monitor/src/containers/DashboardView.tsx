import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import {
  getDashboardAutoScale,
  getDevices,
  getSensors,
  toggleAutoScale,
} from "../reducers/measurementReducer";
import { useApiHook } from "../hooks/apiHook";
import { Box, Button } from "@mui/material";
import moment from "moment";
import { MeasurementsViewModel } from "../models/measurementsBySensor";
import { MultiSensorGraph } from "../components/MultiSensorGraph";

enum TimeSelections {
  Hour24 = 24,
  Hour48 = 48,
  Hour72 = 72,
}

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

  const [timeRange, setTimeRange] = useState<TimeSelections>(
    TimeSelections.Hour48
  );

  const handleTimeRangeChange = (selection: TimeSelections) => {
    setTimeRange(selection);
    const momentStart = moment()
      .local(true)
      .add(-1 * selection, "hour")
      .utc();

    const start = new Date(
      Date.UTC(
        momentStart.year(),
        momentStart.month(),
        momentStart.date(),
        momentStart.hour(),
        momentStart.minute(),
        momentStart.second(),
        0
      )
    );
    setIsLoading(true);
    measurementApiHook
      .getMeasurementsBySensor(
        sensors.map((x) => x.id),
        start,
        undefined
      )
      .then((res) => {
        console.info(res);
        setViewModel(res);
      })
      .catch((er) => {
        console.error(er);
        setViewModel(undefined);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  useEffect(() => {
    if (sensors.length > 0 && !viewModel) {
      const momentStart = moment().local(true).add(-1, "day").utc();
      // TODO implement a better way
      const start = new Date(
        Date.UTC(
          momentStart.year(),
          momentStart.month(),
          momentStart.date(),
          0,
          0,
          0
        )
      );

      setIsLoading(true);
      measurementApiHook
        .getMeasurementsBySensor(
          sensors.map((x) => x.id),
          start,
          undefined
        )
        .then((res) => {
          console.info(res);
          setViewModel(res);
        })
        .catch((er) => {
          console.error(er);
          setViewModel(undefined);
        })
        .finally(() => {
          setIsLoading(false);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [sensors, viewModel]);

  return (
    <AppContentWrapper
      titleParts={[{ text: "Dashboard" }]}
      isLoading={isLoading}
      titleComponent={
        <Box
          sx={{
            display: "flex",
            justifyContent: "start",
            alignItems: "start",
            gap: 2, // Space between buttons
          }}
        >
          <Button
            size="small"
            onClick={() => {
              handleTimeRangeChange(TimeSelections.Hour24);
            }}
            variant={
              timeRange === TimeSelections.Hour24 ? "contained" : "outlined"
            }
          >
            24 h
          </Button>
          <Button
            size="small"
            variant={
              timeRange === TimeSelections.Hour48 ? "contained" : "outlined"
            }
            onClick={() => {
              handleTimeRangeChange(TimeSelections.Hour48);
            }}
          >
            48 h
          </Button>
          <Button
            size="small"
            onClick={() => {
              handleTimeRangeChange(TimeSelections.Hour72);
            }}
            variant={
              timeRange === TimeSelections.Hour72 ? "contained" : "outlined"
            }
          >
            72 h
          </Button>
        </Box>
      }
    >
      <Box
        sx={{
          display: "flex",
          gap: 1, // Space between grid items
          padding: 1, // Padding around the grid container
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
          {devices.map((device) => (
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
                sensors={sensors}
                device={device}
                model={viewModel}
                minHeight={400}
                titleAsLink
                useAutoScale={autoScaleIds.some((s) => s === device.id)}
                onSetAutoScale={(state) => {
                  dispatch(
                    toggleAutoScale({ deviceId: device.id, state: state })
                  );
                }}
              />
            </Box>
          ))}
        </Box>
      </Box>
    </AppContentWrapper>
  );
};
