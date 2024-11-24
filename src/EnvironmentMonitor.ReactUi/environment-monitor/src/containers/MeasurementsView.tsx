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
import { LineChart } from "@mui/x-charts";
import { Measurement } from "../models/measurement";
import { MeasurementTypes } from "../enums/temperatureTypes";

export const MeasurementsView: React.FC = () => {
  // const user = useSelector(getUserInfo);

  const dispatch = useDispatch();
  const isLoggedIn = useSelector(getIsLoggedIn);
  const measurementApiHook = useApiHook().measureHook;
  const [isLoading, setIsLoading] = useState(false);

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

  const hasHumidity = measurements.some(
    (d) => d.typeId === MeasurementTypes.Humidity
  );

  const getMeasurements = () => {
    const temp = measurements
      .filter((x) => x.typeId === MeasurementTypes.Temperature)
      .map((x) => {
        // Get humidity, assume same datetime
        const humidityRow = measurements.find(
          (m) =>
            m.timestamp === x.timestamp &&
            m.typeId === MeasurementTypes.Humidity
        );
        return {
          timestamp: new Date(x.timestamp),
          temperature: x.sensorValue,
          humidity: humidityRow?.sensorValue,
        };
      });
    return temp;
  };
  return (
    <AppContentWrapper
      titleParts={[{ text: "Measurements" }]}
      isLoading={isLoading}
      leftMenu={
        <MeasurementsLeftView
          onSearch={(from: Date, to: Date, sensorId: number) => {
            console.log(to);
            setIsLoading(true);
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
      <LineChart
        dataset={getMeasurements()}
        xAxis={[{ dataKey: "timestamp", scaleType: "time" }]}
        series={
          hasHumidity
            ? [
                {
                  dataKey: "temperature",
                  label: "Temperature °C",
                  showMark: false,
                },
                { dataKey: "humidity", label: "Humidity %", showMark: false },
              ]
            : [
                {
                  dataKey: "temperature",
                  label: "Temperature °C",
                  showMark: false,
                },
              ]
        }
      />
    </AppContentWrapper>
  );
};
