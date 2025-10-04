import React, { useEffect, useState } from "react";
import { type MeasurementsByLocation } from "../models/measurementsBySensor";

import { Box } from "@mui/material";
import { MultiSensorGraph } from "./MultiSensorGraph";
import { useApiHook } from "../hooks/apiHook";
import moment from "moment";
import { type LocationModel } from "../models/location";
import { ChartJsColorsPluginMaxDatasets } from "../models/applicationConstants";
import { useInView } from "react-intersection-observer";

export const DashboardLocationGraph: React.FC<{
  location: LocationModel;
  model: MeasurementsByLocation | undefined;
  timeRange: number;
  autoFetch: boolean;
}> = ({ location, model, timeRange, autoFetch }) => {
  const measurementApiHook = useApiHook().measureHook;

  const [isLoading, setIsLoading] = useState(false);
  const [lastTimeRange, setLastTimeRange] = useState<number | undefined>(
    undefined
  );

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

    if (inView && timeRange !== lastTimeRange) {
      fetchMeasurements();
    } else if (!inView && timeRange !== lastTimeRange) {
      setMeasurementModel(undefined); // Clears the "old" measurements
      setLastTimeRange(undefined);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeRange, inView, autoFetch]);

  const fetchMeasurements = () => {
    setIsLoading(true);
    const momentStart = moment()
      .local(true)
      .add(-1 * timeRange, "hour")
      .utc(true);
    measurementApiHook
      .getMeasurementsByLocation([location.identifier], momentStart)
      .then((res) => {
        setLastTimeRange(timeRange);
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
        devices={undefined}
        model={measurementModel ?? model}
        minHeight={400}
        isLoading={isLoading}
        onRefresh={fetchMeasurements}
        useAutoScale={true}
        title={location.name}
        useDynamicColors={
          model && model.sensors.length > ChartJsColorsPluginMaxDatasets
        }
      />
    </Box>
  );
};
