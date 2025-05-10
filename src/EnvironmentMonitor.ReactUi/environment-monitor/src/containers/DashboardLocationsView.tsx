import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import {
  getDashboardTimeRange,
  getLocations,
  setDashboardTimeRange,
} from "../reducers/measurementReducer";
import { useApiHook } from "../hooks/apiHook";
import { Box } from "@mui/material";
import moment from "moment";
import { type MeasurementsByLocationModel } from "../models/measurementsBySensor";
import { TimeRangeSelectorComponent } from "../components/TimeRangeSelectorComponent";
import { DashboardLocationGraph } from "../components/DashboardLocationGraph";

export const DashbordLocationsView: React.FC = () => {
  const measurementApiHook = useApiHook().measureHook;
  const dispatch = useDispatch();
  const [viewModel, setViewModel] = useState<
    MeasurementsByLocationModel | undefined
  >(undefined);
  const [isLoading, setIsLoading] = useState(false);

  const locations = useSelector(getLocations);

  const timeRange = useSelector(getDashboardTimeRange);

  const handleTimeRangeChange = (selection: number) => {
    dispatch(setDashboardTimeRange(selection));
  };

  useEffect(() => {
    if (!locations || locations.length === 0) {
      console.log("Returning");
      return;
    }
    setIsLoading(true);
    const momentStart = moment()
      .local(true)
      .add(-1 * timeRange, "hour")
      .utc(true);
    measurementApiHook
      .getMeasurementsByLocation(
        locations.map((l) => l.id),
        momentStart,
        undefined
      )
      .then((res) => {
        setViewModel(res); // Set to false once the model is formed
      })
      .catch((er) => {
        console.error(er);
        setViewModel(undefined);
      })
      .finally(() => {
        setIsLoading(false);
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeRange, locations]);

  return (
    <AppContentWrapper
      title="Dashboard - Locations"
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
          display: "grid",
          gridTemplateColumns:
            viewModel?.measurements && viewModel?.measurements.length > 3
              ? {
                  xs: "1fr",
                  lg: "1fr 1fr",
                }
              : "1fr",
          gap: 1,
          padding: 1,
          flexGrow: 1,
          height: "100%",
        }}
      >
        {viewModel?.measurements.map((m) => {
          const location = locations.find((l) => l.id === m.id);
          return (
            <DashboardLocationGraph
              location={location!}
              timeRange={timeRange}
              model={m}
              key={m.id}
            />
          );
        })}
      </Box>
    </AppContentWrapper>
  );
};
