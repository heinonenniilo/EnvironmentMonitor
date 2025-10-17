import { useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { getDevices, getSensors } from "../reducers/measurementReducer";
import { useApiHook } from "../hooks/apiHook";
import moment from "moment";
import {
  MeasurementsInfoTable,
  type MeasurementInfo,
} from "../components/MeasurementsInfoTable";
import { MeasurementTypes } from "../enums/measurementTypes";
import { getDatasetLabel } from "../utilities/measurementUtils";
import { getUserInfo } from "../reducers/userReducer";
import type { MeasurementsViewModel } from "../models/measurementsBySensor";
import type { Device } from "../models/device";
import { dateTimeSort } from "../utilities/datetimeUtils";

export const HomeView: React.FC = () => {
  const sensors = useSelector(getSensors);
  const devices = useSelector(getDevices);
  const [model, setModel] = useState<MeasurementsViewModel | undefined>(
    undefined
  );
  const userInfo = useSelector(getUserInfo);

  const [isLoading, setIsLoading] = useState(false);
  const hook = useApiHook().measureHook;

  const getSensorLabel = (
    sensorIdentifier: string,
    typeId?: MeasurementTypes
  ) => {
    const sensorName =
      sensors?.find((s) => s.identifier === sensorIdentifier)?.name ??
      `${sensorIdentifier}`;
    return getDatasetLabel(sensorName, typeId);
  };

  const getInfoRows = () => {
    if (!model) {
      return [];
    }

    const returnRows: MeasurementInfo[] = [];
    model.measurements.forEach((m) => {
      for (const item in MeasurementTypes) {
        const val = parseInt(MeasurementTypes[item]) as MeasurementTypes;
        if (m.measurements.some((m) => m.typeId === val)) {
          const matchingSensor = sensors.find(
            (s) => s.identifier === m.sensorIdentifier
          );
          const matchingDevice = devices.find(
            (s) => s.identifier === matchingSensor?.deviceIdentifier
          );
          returnRows.push({
            latest: m.latestValues[val],
            min: m.minValues[val],
            max: m.maxValues[val],
            label: getSensorLabel(m.sensorIdentifier, val),
            sensor: matchingSensor,
            device: matchingDevice,
          });
        }
      }
    });
    const sorted = [...returnRows].sort((a, b) =>
      dateTimeSort(a.latest.timestamp, b.latest.timestamp)
    );
    let lastDevice: Device | undefined;
    for (const row of sorted) {
      if (row.device?.identifier !== lastDevice?.identifier) {
        row.boldDevice = true;
        row.renderLinkToDevice = true;
      } else {
        row.hideDevice = true;
      }
      lastDevice = row.device;
    }
    return sorted;
  };

  const getTitle = () => {
    return `Hello, ${userInfo?.email}`;
  };

  useEffect(() => {
    if (devices.length > 0 && model === undefined && hook) {
      setIsLoading(true);
      hook
        .getMeasurementsBySensor(
          [],
          moment(),
          undefined,
          true,
          devices.map((d) => d.identifier)
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
  }, [devices, model]);

  return (
    <AppContentWrapper title={getTitle()} isLoading={isLoading}>
      <p>Latest measurements</p>
      <MeasurementsInfoTable
        infoRows={getInfoRows()}
        hideMax
        hideMin
        showSeconds
      />
    </AppContentWrapper>
  );
};
