import { useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { getDevices } from "../reducers/measurementReducer";
import moment from "moment";
import { Box } from "@mui/material";
import type { GetDeviceMessagesModel } from "../models/getDeviceMessagesModel";
import { DeviceMessagesTable } from "../components/DeviceMessageTable";
import { DeviceMessagesLeftView } from "../components/DeviceMessagesLeftView";

export const DeviceMessagesView: React.FC = () => {
  const devices = useSelector(getDevices);

  const [isLoading, setIsLoading] = useState(false);
  const [getModel, setGetModel] = useState<GetDeviceMessagesModel | undefined>(
    undefined
  );

  useEffect(() => {
    const defaultStart = moment()
      .local(true)
      .add(-1 * 7, "day")
      .utc(true);
    setGetModel({
      deviceIds: devices.map((d) => d.id),
      pageNumber: 1,
      pageSize: 25,
      isDescending: true,
      from: defaultStart,
    });
  }, [devices]);

  return (
    <AppContentWrapper
      title={"Device message"}
      isLoading={isLoading}
      leftMenu={
        getModel && (
          <DeviceMessagesLeftView
            onSearch={(model) => {
              setGetModel(model);
            }}
            model={getModel}
          />
        )
      }
    >
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
          />
        )}
      </Box>
    </AppContentWrapper>
  );
};
