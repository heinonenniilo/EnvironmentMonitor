import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React from "react";
import {
  getDashboardTimeRange,
  getLocations,
  setDashboardTimeRange,
} from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { TimeRangeSelectorComponent } from "../components/TimeRangeSelectorComponent";
import { DashboardLocationGraph } from "../components/DashboardLocationGraph";

export const DashbordLocationsView: React.FC = () => {
  const dispatch = useDispatch();

  const locations = useSelector(getLocations);

  const timeRange = useSelector(getDashboardTimeRange);

  const handleTimeRangeChange = (selection: number) => {
    dispatch(setDashboardTimeRange(selection));
  };

  const visibleLocations = locations.filter((l) => l.visible);
  return (
    <AppContentWrapper
      title="Dashboard - Locations"
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
            visibleLocations && visibleLocations.length > 3
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
        {visibleLocations?.map((m) => {
          const location = locations.find((l) => l.identifier === m.identifier);
          return (
            <DashboardLocationGraph
              location={location!}
              timeRange={timeRange}
              model={undefined}
              key={m.identifier}
              autoFetch
            />
          );
        })}
      </Box>
    </AppContentWrapper>
  );
};
