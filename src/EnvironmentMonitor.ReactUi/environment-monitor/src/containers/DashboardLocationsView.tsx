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
            locations && locations.length > 3
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
        {locations?.map((m) => {
          const location = locations.find((l) => l.id === m.id);
          return (
            <DashboardLocationGraph
              location={location!}
              timeRange={timeRange}
              model={undefined}
              key={m.id}
              autoFetch
            />
          );
        })}
      </Box>
    </AppContentWrapper>
  );
};
