import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import {
  getDeviceInfos,
  getLocations,
  getSensors,
  setDeviceInfos,
} from "../reducers/measurementReducer";
import moment from "moment";
import { Box } from "@mui/material";
import type { GetDeviceMessagesModel } from "../models/getDeviceMessagesModel";
import { DeviceMessagesTable } from "../components/DeviceMessageTable";
import { DeviceMessagesLeftView } from "../components/DeviceMessagesLeftView";
import { useApiHook } from "../hooks/apiHook";
import type { DeviceMessage } from "../models/deviceMessage";
import type { MeasurementsModel } from "../models/measurementsBySensor";
import { MeasurementsDialog } from "../components/MeasurementsDialog";
import { useLocation } from "react-router";
export const defaultStart = moment()
  .local(true)
  .add(-1 * 7, "day")
  .utc(true);

export interface DeviceMessagesLocationState {
  deviceIdentifier?: string;
}

export const DeviceMessagesView: React.FC = () => {
  const deviceInfos = useSelector(getDeviceInfos);
  const dispatch = useDispatch();
  const locations = useSelector(getLocations);
  const apiHook = useApiHook();
  const [isLoading, setIsLoading] = useState(false);
  const sensors = useSelector(getSensors);
  const [getModel, setGetModel] = useState<GetDeviceMessagesModel | undefined>(
    undefined
  );
  const [measurementsModel, setMeasurementsModel] = useState<
    MeasurementsModel | undefined
  >(undefined);

  const [dialogTitle, setDialogTitle] = useState<string | undefined>(undefined);
  const location = useLocation();

  const deviceId = (location.state as DeviceMessagesLocationState | null)
    ?.deviceIdentifier;

  useEffect(() => {
    if (deviceInfos.length > 0) {
      return;
    }

    if (apiHook.deviceHook) {
      apiHook.deviceHook
        .getDeviceInfos()
        .then((res) => {
          if (res) {
            dispatch(setDeviceInfos(res));
          }
        })
        .catch((er) => {
          console.error(er);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [deviceInfos]);

  useEffect(() => {
    const selectedDevice =
      deviceId !== undefined
        ? deviceInfos.find((d) => d.device.identifier === deviceId)
        : undefined;

    const selectedLocation = locations.find(
      (l) => l.identifier === selectedDevice?.device.locationIdentifier
    );

    setGetModel({
      deviceIdentifiers: selectedDevice
        ? [selectedDevice.device.identifier]
        : deviceInfos.map((d) => d.device.identifier),
      locationIdentifiers: selectedLocation
        ? [selectedLocation.identifier]
        : locations.map((l) => l.identifier),
      pageNumber: 0,
      pageSize: 50,
      isDescending: true,
      from: defaultStart,
    });
  }, [deviceInfos, locations, deviceId]);

  const handleClickRow = (message: DeviceMessage) => {
    setIsLoading(true);

    const matchingDevice = deviceInfos.find(
      (d) => d.device.identifier === message.deviceIdentifier
    );
    if (matchingDevice) {
      setDialogTitle(`${matchingDevice.device.name} - ${message.identifier}`);
    } else {
      setDialogTitle(`${message.identifier}`);
    }
    apiHook.measureHook
      .getMeasurements({
        deviceMessageIdentifiers: [message.identifier],
        sensorIdentifiers: [],
        deviceIdentifiers: [message.deviceIdentifier],
      })
      .then((res) => {
        setMeasurementsModel(res);
      })
      .catch((ex) => {
        console.error(ex);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  return (
    <AppContentWrapper
      title={"Device messages"}
      isLoading={isLoading}
      leftMenu={
        getModel && (
          <DeviceMessagesLeftView
            onSearch={(model) => {
              setGetModel({ ...model });
            }}
            model={getModel}
            devices={deviceInfos}
            locations={locations}
          />
        )
      }
    >
      <MeasurementsDialog
        isOpen={measurementsModel !== undefined}
        measurements={measurementsModel?.measurements ?? []}
        onClose={() => {
          setMeasurementsModel(undefined);
          setDialogTitle(undefined);
        }}
        sensors={sensors}
        title={dialogTitle}
      />
      <Box
        display="flex"
        flexDirection="column"
        height="100%"
        flex={1}
        minHeight={0}
      >
        {getModel && (
          <DeviceMessagesTable
            model={getModel}
            onLoadingChange={(state) => setIsLoading(state)}
            onRowClick={handleClickRow}
            deviceInfos={deviceInfos}
            renderLinkToDevice
          />
        )}
      </Box>
    </AppContentWrapper>
  );
};
