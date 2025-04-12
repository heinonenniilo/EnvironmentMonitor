import React, { useState } from "react";
import { MeasurementsByLocation } from "../models/measurementsBySensor";

import { Box, CircularProgress } from "@mui/material";
import { MultiSensorGraph } from "./MultiSensorGraph";
import { useApiHook } from "../hooks/apiHook";
import moment from "moment";
import { LocationModel } from "../models/location";

export const DashboardLocationGraph: React.FC<{
  location: LocationModel;
  model: MeasurementsByLocation | undefined;
  timeRange: number;
}> = ({ location, model, timeRange }) => {
  const measurementApiHook = useApiHook().measureHook;

  const [isLoading, setIsLoading] = useState(false);

  // const dispatch = useDispatch();

  const onRefresh = () => {
    setIsLoading(true);
    const momentStart = moment()
      .local(true)
      .add(-1 * timeRange, "hour")
      .utc(true);
    measurementApiHook
      .getMeasurementsByLocation([location.id], momentStart)
      .then((res) => {})
      .finally(() => {
        setIsLoading(false);
      });
  };

  console.log(location?.locationSensors);
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
        model={model}
        minHeight={400}
        titleAsLink
        isLoading={isLoading}
      />
    </Box>
  );
};
