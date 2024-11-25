import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { getIsLoggedIn } from "../reducers/userReducer";
import { useApiHook } from "../hooks/apiHook";
import { MeasurementsLeftView } from "../components/MeasurementsLeftView";
import {
  getDevices,
  getSensors,
  setDevices,
  setSensors,
} from "../reducers/measurementReducer";
import { Measurement } from "../models/measurement";
import { MeasurementGraph } from "../components/MeasurementGraph";
import { Box } from "@mui/material";
import { Sensor } from "../models/sensor";

export const MeasurementsView: React.FC = () => {
  // const user = useSelector(getUserInfo);

  const dispatch = useDispatch();
  const isLoggedIn = useSelector(getIsLoggedIn);
  const measurementApiHook = useApiHook().measureHook;
  const [isLoading, setIsLoading] = useState(false);
  const [selectedSensor, setSelectedSensor] = useState<Sensor | undefined>(
    undefined
  );

  const [measurements, setMeasurements] = useState<Measurement[]>([]);

  const devices = useSelector(getDevices);
  const sensors = useSelector(getSensors);

  useEffect(() => {
    if (isLoggedIn && measurementApiHook && devices.length === 0) {
      measurementApiHook.getDevices().then((res) => {
        dispatch(setDevices(res ?? []));
      });
    }
  }, [isLoggedIn, measurementApiHook, devices, dispatch]);

  return (
    <AppContentWrapper
      titleParts={[{ text: "Measurements" }]}
      isLoading={isLoading}
      leftMenu={
        <MeasurementsLeftView
          onSearch={(from: Date, to: Date, sensorId: number) => {
            console.log(to);
            setIsLoading(true);

            const selectedSensor = sensors.find((s) => s.id === sensorId);
            setSelectedSensor(selectedSensor);
            measurementApiHook
              .getMeasurements(sensorId, from, to)
              .then((res) => {
                setMeasurements(res);
              })
              .finally(() => {
                setIsLoading(false);
              });
          }}
          getSensors={(deviceId: string) => {
            measurementApiHook
              .getSensors(deviceId)
              .then((res) => {
                dispatch(setSensors(res));
              })
              .finally(() => {
                setIsLoading(false);
              });
          }}
          devices={devices}
          sensors={sensors}
        />
      }
    >
      <Box
        flexGrow={1}
        flex={1}
        width={"100%"}
        height={"100%"}
        sx={{
          display: "flex",
          flexDirection: "column",
          //flexGrow: {
          //  xs: 1,
          //},
          //columnGap: {
          //  xl: 3,
          //},
        }}
      >
        <MeasurementGraph sensor={selectedSensor} measurements={measurements} />
      </Box>
    </AppContentWrapper>
  );
};
