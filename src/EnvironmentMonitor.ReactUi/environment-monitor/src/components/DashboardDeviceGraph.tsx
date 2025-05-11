import React, { useEffect, useState } from "react";
import { type Device } from "../models/device";
import { type Sensor } from "../models/sensor";
import { type MeasurementsViewModel } from "../models/measurementsBySensor";
import { useDispatch, useSelector } from "react-redux";
import {
  getDeviceAutoScale,
  toggleAutoScale,
} from "../reducers/measurementReducer";
import { Box } from "@mui/material";
import { MultiSensorGraph } from "./MultiSensorGraph";
import { useApiHook } from "../hooks/apiHook";
import moment from "moment";

export const DashboardDeviceGraph: React.FC<{
  device: Device;
  sensors: Sensor[];
  model: MeasurementsViewModel | undefined;
  timeRange: number;
}> = ({ device, sensors, model, timeRange }) => {
  const useAutoScale = useSelector(getDeviceAutoScale(device.id));
  const measurementApiHook = useApiHook().measureHook;

  const [deviceModel, setDeviceModel] = useState<
    MeasurementsViewModel | undefined
  >(undefined);

  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (model) {
      setDeviceModel(undefined);
    }
  }, [model]);
  const dispatch = useDispatch();

  const onRefresh = () => {
    if (sensors.length === 0) {
      return;
    }

    const momentStart = moment()
      .local(true)
      .add(-1 * timeRange, "hour")
      .utc(true);
    setIsLoading(true);
    measurementApiHook
      .getMeasurementsBySensor(
        sensors.map((x) => x.id),
        momentStart,
        undefined
      )
      .then((res) => {
        setDeviceModel(res);
      })
      .catch((er) => {
        console.error(er);
        setDeviceModel(undefined);
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
        sensors={sensors}
        devices={[device]}
        model={deviceModel ? deviceModel : model}
        minHeight={400}
        titleAsLink
        useAutoScale={useAutoScale}
        onSetAutoScale={(state) =>
          dispatch(toggleAutoScale({ deviceId: device.id, state }))
        }
        onRefresh={onRefresh}
        isLoading={isLoading}
      />
    </Box>
  );
};
