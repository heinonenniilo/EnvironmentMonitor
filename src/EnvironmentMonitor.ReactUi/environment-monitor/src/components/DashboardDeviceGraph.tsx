import React, { useEffect, useState } from "react";
import { type Device } from "../models/device";
import { type Sensor } from "../models/sensor";
import { type MeasurementsViewModel } from "../models/measurementsBySensor";
import { useDispatch, useSelector } from "react-redux";
import { useInView } from "react-intersection-observer";
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
  autoFetch: boolean;
}> = ({ device, sensors, model, timeRange, autoFetch }) => {
  const useAutoScale = useSelector(getDeviceAutoScale(device.identifier));
  const measurementApiHook = useApiHook().measureHook;

  const [deviceModel, setDeviceModel] = useState<
    MeasurementsViewModel | undefined
  >(undefined);
  const [isLoading, setIsLoading] = useState(false);
  const [lastTimeRange, setLastTimeRange] = useState<number | undefined>(
    undefined
  );

  const { ref, inView } = useInView({
    triggerOnce: false,
    threshold: 0.5,
  });

  useEffect(() => {
    if (!autoFetch) {
      return;
    }

    if (!sensors || sensors.length === 0) {
      return;
    }

    if (inView && timeRange !== lastTimeRange) {
      fetchMeasurements();
    } else if (!inView && timeRange !== lastTimeRange) {
      setDeviceModel(undefined);
      setLastTimeRange(undefined);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeRange, inView, autoFetch]);
  const dispatch = useDispatch();

  const fetchMeasurements = () => {
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
        sensors.map((x) => x.identifier),
        momentStart,
        undefined
      )
      .then((res) => {
        setLastTimeRange(timeRange);
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
        maxHeight: "600px",
      }}
      ref={ref}
    >
      <MultiSensorGraph
        sensors={sensors}
        entities={[device]}
        model={deviceModel ? deviceModel : model}
        minHeight={400}
        titleAsLink
        useAutoScale={useAutoScale}
        onSetAutoScale={(state) =>
          dispatch(
            toggleAutoScale({ deviceIdentifier: device.identifier, state })
          )
        }
        onRefresh={fetchMeasurements}
        isLoading={isLoading}
        enableHighlightOnRowHover
        showFullScreenIcon
      />
    </Box>
  );
};
