import React, { useEffect, useState } from "react";
import { type MeasurementsByLocation } from "../../models/measurementsBySensor";

import { Box } from "@mui/material";
import { MultiSensorGraph } from "../MultiSensorGraph";
import { useApiHook } from "../../hooks/apiHook";
import moment from "moment";
import { type LocationModel } from "../../models/location";
import { ChartJsColorsPluginMaxDatasets } from "../../models/applicationConstants";
import { useInView } from "react-intersection-observer";

export const DashboardLocationGraph: React.FC<{
  location: LocationModel;
  model: MeasurementsByLocation | undefined;
  timeRange: number;
  autoFetch: boolean;
  measurementTypes?: number[];
}> = ({ location, model, timeRange, autoFetch, measurementTypes }) => {
  const measurementApiHook = useApiHook().measureHook;

  const [isLoading, setIsLoading] = useState(false);
  const [lastTimeRange, setLastTimeRange] = useState<number | undefined>(
    undefined
  );
  const [lastMeasurementTypes, setLastMeasurementTypes] = useState<
    number[] | undefined
  >(undefined);

  const [measurementModel, setMeasurementModel] = useState<
    MeasurementsByLocation | undefined
  >(undefined);

  const { ref, inView } = useInView({
    triggerOnce: false,
    threshold: 0.5,
  });

  useEffect(() => {
    if (!autoFetch) {
      return;
    }

    const measurementTypesChanged =
      JSON.stringify(measurementTypes) !== JSON.stringify(lastMeasurementTypes);

    if (inView && (timeRange !== lastTimeRange || measurementTypesChanged)) {
      fetchMeasurements();
    } else if (
      !inView &&
      (timeRange !== lastTimeRange || measurementTypesChanged)
    ) {
      setMeasurementModel(undefined); // Clears the "old" measurements
      setLastTimeRange(undefined);
      setLastMeasurementTypes(undefined);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeRange, inView, autoFetch, measurementTypes]);

  const fetchMeasurements = () => {
    setIsLoading(true);
    const momentStart = moment()
      .local(true)
      .add(-1 * timeRange, "hour")
      .utc(true);
    measurementApiHook
      .getMeasurementsByLocation(
        [location.identifier],
        momentStart,
        undefined,
        undefined,
        undefined,
        measurementTypes && measurementTypes.length > 0
          ? measurementTypes
          : undefined
      )
      .then((res) => {
        setLastTimeRange(timeRange);
        setLastMeasurementTypes(measurementTypes);
        setMeasurementModel(res?.measurements[0]);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };
  return (
    <Box
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
        position: "relative",
        maxHeight: "650px",
      }}
      ref={ref}
    >
      <MultiSensorGraph
        sensors={measurementModel?.sensors ?? model?.sensors}
        entities={[location]}
        model={measurementModel ?? model}
        minHeight={400}
        isLoading={isLoading}
        onRefresh={fetchMeasurements}
        useAutoScale={true}
        title={location.name}
        titleAsLink
        linkToLocationMeasurements
        useDynamicColors={
          model && model.sensors.length > ChartJsColorsPluginMaxDatasets
        }
        enableHighlightOnRowHover
        showFullScreenIcon
      />
    </Box>
  );
};
