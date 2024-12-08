import { useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { getDevices, getSensors } from "../reducers/measurementReducer";
import { MeasurementsViewModel } from "../models/measurementsBySensor";
import { useApiHook } from "../hooks/apiHook";
import moment from "moment";
import {
  MeasurementInfo,
  MeasurementsInfoTable,
} from "../components/MeasurementsInfoTable";
import { MeasurementTypes } from "../enums/temperatureTypes";
import { getMeasurementUnit } from "../utilities/measurementUtils";
import { getUserInfo } from "../reducers/userReducer";

export const HomeView: React.FC = () => {
  const sensors = useSelector(getSensors);
  const devices = useSelector(getDevices);
  const [model, setModel] = useState<MeasurementsViewModel | undefined>(
    undefined
  );
  const userInfo = useSelector(getUserInfo);

  const [isLoading, setIsLoading] = useState(false);
  const hook = useApiHook().measureHook;

  const getSensorLabel = (sensorId: number, typeId?: MeasurementTypes) => {
    const sensorName =
      sensors?.find((s) => s.id === sensorId)?.name ?? `${sensorId}`;

    if (!typeId) {
      return sensorName;
    } else {
      return `${sensorName} ${getMeasurementUnit(typeId)}`;
    }
  };

  const getInfoRows = () => {
    if (!model) {
      return [];
    }

    const returnRows: MeasurementInfo[] = [];
    model.measurements.forEach((m) => {
      for (let item in MeasurementTypes) {
        let val = parseInt(MeasurementTypes[item]) as MeasurementTypes;
        if (m.measurements.some((m) => m.typeId === val)) {
          const matchingSensor = sensors.find((s) => s.id === m.sensorId);
          const matchingDevice = devices.find(
            (s) => s.id === matchingSensor?.deviceId
          );
          returnRows.push({
            latest: m.latestValues[val],
            min: m.minValues[val],
            max: m.maxValues[val],
            label: getSensorLabel(m.sensorId, val),
            sensor: matchingSensor,
            device: matchingDevice,
          });
        }
      }
    });
    const sorted = [...returnRows].sort((a, b) => {
      let res =
        new Date(b.latest.timestamp).getTime() -
        new Date(a.latest.timestamp).getTime();
      return res;
    });
    return sorted;
  };

  const getTitle = () => {
    return `Hello, ${userInfo?.email}`;
  };

  useEffect(() => {
    if (sensors.length > 0 && model === undefined && hook) {
      setIsLoading(true);
      hook
        .getMeasurementsBySensor(
          sensors.map((s) => s.id),
          moment(),
          undefined,
          true
        )
        .then((res) => {
          setModel(res);
        })
        .catch((er) => {
          //
          setModel(undefined);
          console.error(er);
        })
        .finally(() => {
          setIsLoading(false);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [sensors, model]);

  return (
    <AppContentWrapper
      titleParts={[{ text: getTitle() }]}
      isLoading={isLoading}
    >
      <p>Latest measurements</p>
      <MeasurementsInfoTable infoRows={getInfoRows()} hideMax hideMin />
    </AppContentWrapper>
  );
};
