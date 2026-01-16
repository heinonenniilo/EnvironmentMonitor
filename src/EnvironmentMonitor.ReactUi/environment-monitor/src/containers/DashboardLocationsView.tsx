import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useState } from "react";
import {
  getDashboardTimeRange,
  getLocations,
  setDashboardTimeRange,
} from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { TimeRangeSelectorComponent } from "../components/TimeRangeSelectorComponent";
import { DashboardLocationGraph } from "../components/Dashboard/DashboardLocationGraph";
import { DashboardLeftMenu } from "../components/Dashboard/DashboardLeftMenu";
import { MeasurementTypes } from "../enums/measurementTypes";

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

export const DashbordLocationsView: React.FC = () => {
  const dispatch = useDispatch();
  const [selectedMeasurementTypes, setSelectedMeasurementTypes] = useState<
    number[]
  >(getAvailableMeasurementTypes());

  const locations = useSelector(getLocations);

  const timeRange = useSelector(getDashboardTimeRange);

  const handleTimeRangeChange = (selection: number) => {
    dispatch(setDashboardTimeRange(selection));
  };

  const handleMeasurementTypesChange = (measurementTypes: number[]) => {
    setSelectedMeasurementTypes(measurementTypes);
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
      leftMenu={
        <DashboardLeftMenu
          onMeasurementTypesChange={handleMeasurementTypesChange}
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
