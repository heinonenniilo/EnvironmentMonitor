import React from "react";
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";
import { MeasurementsViewModel } from "../models/measurementsBySensor";
import { useDispatch, useSelector } from "react-redux";
import {
  getDeviceAutoScale,
  toggleAutoScale,
} from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { MultiSensorGraph } from "./MultiSensorGraph";

export const DashboardDeviceGraph: React.FC<{
  device: Device;
  sensors: Sensor[];
  model: MeasurementsViewModel | undefined;
}> = ({ device, sensors, model }) => {
  const useAutoScale = useSelector(getDeviceAutoScale(device.id));
  const dispatch = useDispatch();
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
      }}
    >
      <MultiSensorGraph
        sensors={sensors}
        devices={[device]}
        model={model}
        minHeight={400}
        titleAsLink
        useAutoScale={useAutoScale}
        onSetAutoScale={(state) =>
          dispatch(toggleAutoScale({ deviceId: device.id, state }))
        }
      />
    </Box>
  );
};
