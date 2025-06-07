import { useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { getDevices, getSensors } from "../reducers/measurementReducer";
import moment from "moment";
import { Box } from "@mui/material";
import type { GetDeviceMessagesModel } from "../models/getDeviceMessagesModel";
import { DeviceMessagesTable } from "../components/DeviceMessageTable";
import { DeviceMessagesLeftView } from "../components/DeviceMessagesLeftView";
import { useApiHook } from "../hooks/apiHook";
import type { DeviceMessage } from "../models/deviceMessage";
import type { MeasurementsModel } from "../models/measurementsBySensor";
import { MeasurementsDialog } from "../components/MeasurementsDialog";
export const defaultStart = moment()
  .local(true)
  .add(-1 * 7, "day")
  .utc(true);

export const DeviceMessagesView: React.FC = () => {
  const devices = useSelector(getDevices);
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

  useEffect(() => {
    setGetModel({
      deviceIds: devices.map((d) => d.id),
      pageNumber: 0,
      pageSize: 50,
      isDescending: true,
      from: defaultStart,
    });
  }, [devices]);

  const handleClickRow = (message: DeviceMessage) => {
    setIsLoading(true);

    const matchingDevice = devices.find((d) => d.id === message.deviceId);
    if (matchingDevice) {
      setDialogTitle(`${matchingDevice.name} - ${message.identifier}`);
    } else {
      setDialogTitle(`${message.identifier}`);
    }
    apiHook.measureHook
      .getMeasurements({
        deviceMessageIds: [message.id],
        sensorIds: [],
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
            devices={devices}
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
          />
        )}
      </Box>
    </AppContentWrapper>
  );
};
