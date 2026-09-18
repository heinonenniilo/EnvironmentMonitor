import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import {
  getDashboardTimeRange,
  getLocations,
  getSelectedMeasurementTypes,
  setDashboardTimeRange,
  setSelectedMeasurementTypes,
} from "../reducers/measurementReducer";
import { Box, IconButton, Tooltip } from "@mui/material";
import { Refresh } from "@mui/icons-material";
import { TimeRangeSelectorComponent } from "../components/Measurements/TimeRangeSelectorComponent";
import { DashboardLocationGraph } from "../components/Dashboard/DashboardLocationGraph";
import { DashboardLeftMenu } from "../components/Dashboard/DashboardLeftMenu";
import { toggleLeftMenuOpen } from "../reducers/userInterfaceReducer";

export const DashbordLocationsView: React.FC = () => {
  const dispatch = useDispatch();
  const selectedMeasurementTypes = useSelector(getSelectedMeasurementTypes);

  const locations = useSelector(getLocations);

  const timeRange = useSelector(getDashboardTimeRange);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const handleTimeRangeChange = (selection: number) => {
    dispatch(setDashboardTimeRange(selection));
  };

  const handleRefresh = () => {
    setRefreshTrigger((previous) => previous + 1);
  };

  const visibleLocations = locations.filter((l) => l.visible);

  useEffect(() => {
    dispatch(toggleLeftMenuOpen(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <AppContentWrapper
      title="Dashboard - Locations"
      titleComponent={
        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
          <TimeRangeSelectorComponent
            timeRange={timeRange}
            onSelectTimeRange={handleTimeRangeChange}
          />
          <Tooltip title="Refresh">
            <IconButton onClick={handleRefresh} size="medium">
              <Refresh />
            </IconButton>
          </Tooltip>
        </Box>
      }
      leftMenu={
        <DashboardLeftMenu
          selectedMeasurementTypes={selectedMeasurementTypes}
          onMeasurementTypesChange={(types) =>
            dispatch(setSelectedMeasurementTypes(types))
          }
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
              refreshTrigger={refreshTrigger}
              autoFetch
              measurementTypes={
                selectedMeasurementTypes.length > 0
                  ? selectedMeasurementTypes
                  : undefined
              }
            />
          );
        })}
      </Box>
    </AppContentWrapper>
  );
};
