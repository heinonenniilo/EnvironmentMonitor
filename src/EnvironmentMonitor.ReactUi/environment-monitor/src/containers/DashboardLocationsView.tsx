import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect } from "react";
import {
  getDashboardTimeRange,
  getLocations,
  getSelectedMeasurementTypes,
  setDashboardTimeRange,
  setSelectedMeasurementTypes,
} from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { TimeRangeSelectorComponent } from "../components/TimeRangeSelectorComponent";
import { DashboardLocationGraph } from "../components/Dashboard/DashboardLocationGraph";
import { DashboardLeftMenu } from "../components/Dashboard/DashboardLeftMenu";
import { toggleLeftMenuOpen } from "../reducers/userInterfaceReducer";

export const DashbordLocationsView: React.FC = () => {
  const dispatch = useDispatch();
  const selectedMeasurementTypes = useSelector(getSelectedMeasurementTypes);

  const locations = useSelector(getLocations);

  const timeRange = useSelector(getDashboardTimeRange);

  const handleTimeRangeChange = (selection: number) => {
    dispatch(setDashboardTimeRange(selection));
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
        <TimeRangeSelectorComponent
          timeRange={timeRange}
          onSelectTimeRange={handleTimeRangeChange}
        />
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
