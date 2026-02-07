import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { useApiHook } from "../hooks/apiHook";
import { MeasurementsLeftView } from "../components/MeasurementsLeftView";
import {
  getDashboardTimeRange,
  getSelectedMeasurementTypes,
  setSelectedMeasurementTypes,
} from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { type MeasurementsViewModel } from "../models/measurementsBySensor";
import { MultiSensorGraph } from "../components/MultiSensorGraph";
import { type Sensor } from "../models/sensor";
import moment from "moment";

export const PublicSensorMeasurementsView: React.FC = () => {
  const measurementApiHook = useApiHook().measureHook;
  const dispatch = useDispatch();
  const [isLoading, setIsLoading] = useState(false);
  const [isFullScreen, setIsFullScreen] = useState(false);

  const [titleToShow, setTitleToShow] = useState<string | undefined>(undefined);
  const [measurementsModel, setMeasurementsModel] = useState<
    MeasurementsViewModel | undefined
  >(undefined);

  const [timeFrom, setTimeFrom] = useState<moment.Moment | undefined>(
    undefined,
  );
  const [timeTo, setTimeTo] = useState<moment.Moment | undefined>(undefined);
  const dashboardTimeRange = useSelector(getDashboardTimeRange);
  const selectedMeasurementTypes = useSelector(getSelectedMeasurementTypes);

  // Available public sensors fetched from backend
  const [availableSensors, setAvailableSensors] = useState<Sensor[]>([]);
  const [selectedSensors, setSelectedSensors] = useState<Sensor[]>([]);

  const toggleSensorSelection = (sensorId: string) => {
    if (selectedSensors.some((s) => s.identifier === sensorId)) {
      setSelectedSensors([
        ...selectedSensors.filter((s) => s.identifier !== sensorId),
      ]);
    } else {
      const matchingSensor = availableSensors.find(
        (s) => s.identifier === sensorId,
      );
      if (matchingSensor) {
        setSelectedSensors([...selectedSensors, matchingSensor]);
      }
    }
  };

  // Fetch available public sensors on mount
  useEffect(() => {
    if (!measurementApiHook) return;

    setIsLoading(true);
    const fromDate = moment()
      .utc(true)
      .add(-1 * dashboardTimeRange, "hour")
      .startOf("day");

    measurementApiHook
      .getPublicMeasurements(fromDate, undefined, true)
      .then((res) => {
        if (res?.sensors) {
          setAvailableSensors(
            res.sensors.map((s) => ({
              ...s,
              parentIdentifier: "public",
            })),
          );
        }
      })
      .finally(() => {
        setIsLoading(false);
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getGraphTitle = (from: moment.Moment, to?: moment.Moment): string => {
    const datesToShow = to
      ? `${from.format("DD.MM.YYYY")} -> ${to.format("DD.MM.YYYY")}`
      : `${from.format("DD.MM.YYYY")} ->`;
    return `Public Sensors: ${datesToShow}`;
  };

  const onSearch = (
    from: moment.Moment,
    to: moment.Moment | undefined,
    sensorIds: string[],
    measurementTypes?: number[],
  ) => {
    setIsLoading(true);
    measurementApiHook
      .getPublicMeasurementsFiltered(
        sensorIds,
        from,
        to,
        measurementTypes && measurementTypes.length > 0
          ? measurementTypes
          : undefined,
      )
      .then((res) => {
        setSelectedSensors(
          availableSensors.filter((sensor) =>
            sensorIds.some((s) => sensor.identifier === s),
          ),
        );
        setMeasurementsModel(res);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  return (
    <AppContentWrapper
      title="Public Measurements"
      isLoading={isLoading}
      leftMenu={
        <MeasurementsLeftView
          onSearch={(
            from: moment.Moment,
            to: moment.Moment | undefined,
            sensorIds: string[],
            measurementTypes?: number[],
          ) => {
            setTimeFrom(from);
            setTimeTo(to);
            setTitleToShow(getGraphTitle(from, to));
            onSearch(from, to, sensorIds, measurementTypes);
          }}
          onSelectEntity={() => {}}
          toggleSensorSelection={toggleSensorSelection}
          selectedEntities={[]}
          selectedSensors={selectedSensors.map((s) => s.identifier)}
          entities={[]}
          sensors={availableSensors}
          timeFrom={timeFrom}
          hideEntitySelector
          selectedMeasurementTypes={selectedMeasurementTypes}
          onMeasurementTypesChange={(types) =>
            dispatch(setSelectedMeasurementTypes(types))
          }
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
          title={titleToShow}
          isLoading={isFullScreen ? isLoading : undefined}
          model={
            measurementsModel
              ? {
                  measurements: measurementsModel.measurements.filter((m) =>
                    selectedSensors.some(
                      (s) => s.identifier === m.sensorIdentifier,
                    ),
                  ),
                }
              : undefined
          }
          key={"graph_01"}
          isFullScreen={isFullScreen}
          onSetFullScreen={setIsFullScreen}
          minHeight={500}
          useAutoScale
          hideUseAutoScale
          onRefresh={() => {
            if (timeFrom) {
              onSearch(
                timeFrom,
                timeTo,
                selectedSensors.map((s) => s.identifier),
                selectedMeasurementTypes ? selectedMeasurementTypes : undefined,
              );
            }
          }}
          showFullScreenIcon
        />
      </Box>
    </AppContentWrapper>
  );
};
