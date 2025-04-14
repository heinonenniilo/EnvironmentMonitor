import React, { useEffect, useState } from "react";
import { MeasurementsByLocation } from "../models/measurementsBySensor";

import { Box } from "@mui/material";
import { MultiSensorGraph } from "./MultiSensorGraph";
import { useApiHook } from "../hooks/apiHook";
import moment from "moment";
import { LocationModel } from "../models/location";
import { ChartJsColorsPluginMaxDatasets } from "../models/applicationConstants";

export const DashboardLocationGraph: React.FC<{
  location: LocationModel;
  model: MeasurementsByLocation | undefined;
  timeRange: number;
}> = ({ location, model, timeRange }) => {
  const measurementApiHook = useApiHook().measureHook;

  const [isLoading, setIsLoading] = useState(false);

  const [measurementModel, setMeasurementModel] = useState<
    MeasurementsByLocation | undefined
  >(undefined);

  useEffect(() => {
    if (model) {
      setMeasurementModel(undefined);
    }
  }, [model]);

  const onRefresh = () => {
    setIsLoading(true);
    const momentStart = moment()
      .local(true)
      .add(-1 * timeRange, "hour")
      .utc(true);
    measurementApiHook
      .getMeasurementsByLocation([location.id], momentStart)
      .then((res) => {
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
      }}
    >
      <MultiSensorGraph
        sensors={model?.sensors}
        devices={undefined}
        model={measurementModel ?? model}
        minHeight={400}
        isLoading={isLoading}
        onRefresh={onRefresh}
        useAutoScale={true}
        title={location.name}
        useDynamicColors={
          model && model.sensors.length > ChartJsColorsPluginMaxDatasets
        }
      />
    </Box>
  );
};
