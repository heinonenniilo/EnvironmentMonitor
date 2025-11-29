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

  const getSensors = () => {
    const toReturn: Sensor[] = [];
    locations.forEach((location) => {
      location.locationSensors.forEach((sensor) => {
        toReturn.push({ ...sensor, deviceIdentifier: location.identifier });
      });
    });
    return toReturn;
  };

  const toggleLocationSelection = (locationId: string) => {
    const matchingLocation = locations.find((l) => l.identifier === locationId);

    if (!selectedLocations.some((l) => l.identifier === locationId)) {
      if (matchingLocation) {
        setSelectedLocations([...selectedLocations, matchingLocation]);
      }
    } else {
      // When deselecting a location, also remove its sensors from selection
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
              // setSelectedSensors(matchingLocation.locationSensors);
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
    locationIds: string[]
  ) => {
    console.log(locationIds);
    setIsLoading(true);
    measurementApiHook
      .getMeasurementsByLocation(locationIds, from, to)
      .then((res) => {
        if (res) {
          /*
          setSelectedSensors(
            sensors.filter((sensor) =>
              locationIds.some((s) => sensor.identifier === s)
            )
          );
          */
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

    return getSensors().filter((s) => sensorIds.has(s.identifier));
  };

  return (
    <AppContentWrapper
      title="Location Measurements"
      isLoading={isLoading}
      leftMenu={
        <MeasurementsLeftView
          onSearch={(
            from: moment.Moment,
            to: moment.Moment | undefined
            // sensorIds: string[]
          ) => {
            setTimeFrom(from);
            setTimeTo(to);
            onSearch(
              from,
              to,
              selectedLocations.map((l) => l.identifier)
            );
          }}
          onSelectDevice={toggleLocationSelection}
          toggleSensorSelection={toggleSensorSelection}
          selectedDevices={selectedLocations}
          selectedSensors={selectedSensors.map((s) => s.identifier)}
          devices={locations.map((l) => {
            return { ...l, displayName: l.name };
          })}
          sensors={getAvailableSensors()}
          timeFrom={timeFrom}
          showsLocations
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
          devices={selectedLocations.map((l) => {
            return { ...l, displayName: l.name };
          })}
          key={"graph_01"}
          minHeight={500}
          useAutoScale
          hideUseAutoScale
          onRefresh={() => {
            if (timeFrom) {
              onSearch(
                timeFrom,
                timeTo,
                selectedSensors.map((s) => s.identifier)
              );
            }
          }}
        />
      </Box>
    </AppContentWrapper>
  );
};
