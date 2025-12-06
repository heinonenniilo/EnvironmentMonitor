import { useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { useApiHook } from "../hooks/apiHook";
import { MeasurementsLeftView } from "../components/MeasurementsLeftView";
import {
  getDashboardTimeRange,
  getLocations,
} from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { type LocationModel } from "../models/location";
import { type MeasurementsByLocationModel } from "../models/measurementsBySensor";
import { MultiSensorGraph } from "../components/MultiSensorGraph";
import { type Sensor } from "../models/sensor";
import { useParams } from "react-router";
import moment from "moment";

export const LocationMeasurementsView: React.FC = () => {
  const measurementApiHook = useApiHook().measureHook;
  const [isLoading, setIsLoading] = useState(false);
  const { locationId } = useParams<{ locationId?: string }>();
  const [selectedLocations, setSelectedLocations] = useState<LocationModel[]>(
    []
  );

  const [measurementsModel, setMeasurementsModel] = useState<
    MeasurementsByLocationModel | undefined
  >(undefined);

  const [timeFrom, setTimeFrom] = useState<moment.Moment | undefined>(
    undefined
  );
  const [timeTo, setTimeTo] = useState<moment.Moment | undefined>(undefined);
  const locations = useSelector(getLocations);
  const sensors = locations.flatMap((l) => l.locationSensors);
  const dashboardTimeRange = useSelector(getDashboardTimeRange);

  const [selectedSensors, setSelectedSensors] = useState<Sensor[]>([]);

  const toggleSensorSelection = (sensorId: string) => {
    if (selectedSensors.some((s) => s.identifier === sensorId)) {
      setSelectedSensors([
        ...selectedSensors.filter((s) => s.identifier !== sensorId),
      ]);
    } else {
      const matchingSensor = sensors.find((s) => s.identifier === sensorId);
      if (matchingSensor) {
        setSelectedSensors([...selectedSensors, matchingSensor]);
      }
    }
  };

  const toggleLocationSelection = (locationId: string) => {
    const matchingLocation = locations.find((l) => l.identifier === locationId);

    if (!selectedLocations.some((l) => l.identifier === locationId)) {
      if (matchingLocation) {
        setSelectedLocations([...selectedLocations, matchingLocation]);
      }
    } else {
      setSelectedSensors(
        selectedSensors.filter(
          (s) =>
            !matchingLocation?.locationSensors.some(
              (ls) => ls.identifier === s.identifier
            )
        )
      );
      setSelectedLocations(
        selectedLocations.filter((l) => l.identifier !== locationId)
      );
    }
  };

  useEffect(() => {
    if (locationId !== undefined && locations.length > 0) {
      const matchingLocation = locations.find(
        (l) => l.identifier === locationId
      );
      if (matchingLocation) {
        setSelectedLocations([matchingLocation]);
        setSelectedSensors(
          matchingLocation.locationSensors.map((s) => {
            return { ...s, deviceIdentifier: matchingLocation.identifier };
          })
        );
        setIsLoading(true);

        const fromDate = moment()
          .utc(true)
          .add(-1 * dashboardTimeRange, "hour")
          .startOf("day");

        setTimeFrom(fromDate);

        measurementApiHook
          .getMeasurementsByLocation(
            [matchingLocation.identifier],
            fromDate,
            undefined
          )
          .then((res) => {
            if (res) {
              setMeasurementsModel(res);
            }
          })
          .finally(() => {
            setIsLoading(false);
          });
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [locationId, locations]);

  const onSearch = (
    from: moment.Moment,
    to: moment.Moment | undefined,
    locationIds: string[],
    sensorIds?: string[]
  ) => {
    setIsLoading(true);
    measurementApiHook
      .getMeasurementsByLocation(locationIds, from, to, false, sensorIds)
      .then((res) => {
        if (res) {
          setMeasurementsModel(res);
        }
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  // Get sensors for selected locations
  const getAvailableSensors = (): Sensor[] => {
    if (selectedLocations.length === 0) {
      return [];
    }

    const sensorIds = new Set<string>();
    selectedLocations.forEach((location) => {
      location.locationSensors.forEach((sensor) => {
        sensorIds.add(sensor.identifier);
      });
    });

    return sensors.filter((s) => sensorIds.has(s.identifier));
  };

  return (
    <AppContentWrapper
      title="Location Measurements"
      isLoading={isLoading}
      leftMenu={
        <MeasurementsLeftView
          onSearch={(
            from: moment.Moment,
            to: moment.Moment | undefined,
            sensorIds: string[]
          ) => {
            setTimeFrom(from);
            setTimeTo(to);
            onSearch(
              from,
              to,
              selectedLocations.map((l) => l.identifier),
              sensorIds
            );
          }}
          onSelectEntity={toggleLocationSelection}
          toggleSensorSelection={toggleSensorSelection}
          selectedEntities={selectedLocations}
          selectedSensors={selectedSensors.map((s) => s.identifier)}
          entities={locations}
          sensors={getAvailableSensors()}
          timeFrom={timeFrom}
          entityName="Location"
        />
      }
    >
      <Box
        flexGrow={1}
        flex={1}
        width={"100%"}
        sx={{
          display: "flex",
          flexDirection: "column",
        }}
      >
        <MultiSensorGraph
          enableFullScreen
          sensors={selectedSensors}
          model={
            measurementsModel
              ? {
                  measurements: measurementsModel.measurements
                    .flatMap(
                      (locationMeasurement) => locationMeasurement.measurements
                    )
                    .filter((m) =>
                      selectedSensors.some(
                        (s) => s.identifier === m.sensorIdentifier
                      )
                    ),
                }
              : undefined
          }
          entities={selectedLocations}
          key={"graph_01"}
          minHeight={500}
          useAutoScale
          hideUseAutoScale
          onRefresh={() => {
            if (timeFrom) {
              onSearch(
                timeFrom,
                timeTo,
                selectedLocations.map((l) => l.identifier),
                selectedSensors.length > 0
                  ? selectedSensors.map((s) => s.identifier)
                  : undefined
              );
            }
          }}
        />
      </Box>
    </AppContentWrapper>
  );
};
