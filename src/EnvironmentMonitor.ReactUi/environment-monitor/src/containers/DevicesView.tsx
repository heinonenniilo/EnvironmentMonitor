import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useEffect, useState } from "react";
import { type Device } from "../models/device";
import { useApiHook } from "../hooks/apiHook";
import { DeviceTable } from "../components/Devices/DeviceTable";
import { dateTimeSort } from "../utilities/datetimeUtils";
import { useDispatch, useSelector } from "react-redux";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";
import {
  getDeviceInfos,
  getLocations,
  setDeviceInfos,
} from "../reducers/measurementReducer";
import { Box, IconButton, Tooltip } from "@mui/material";
import { Add } from "@mui/icons-material";
import { EditDeviceDialog } from "../components/Devices/EditDeviceDialog";
import type { AddOrUpdateDeviceDto } from "../models/addOrUpdateDeviceDto";

export const DevicesView: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [addDeviceDialogOpen, setAddDeviceDialogOpen] = useState(false);
  const dispatch = useDispatch();
  const deviceInfos = useSelector(getDeviceInfos);
  const locations = useSelector(getLocations);
  const deviceHook = useApiHook().deviceHook;

  useEffect(() => {
    getDevices();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getDevices = () => {
    setIsLoading(true);
    deviceHook
      .getDeviceInfos()
      .then((res) => {
        if (res) {
          dispatch(setDeviceInfos(res));
        }
      })
      .catch((er) => {
        console.error(er);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const rebootDevice = (device: Device | undefined) => {
    if (!device) {
      return;
    }
    setIsLoading(true);
    const deviceIdentifier = device.identifier;

    deviceHook
      .rebootDevice(deviceIdentifier)
      .then((r) => {
        if (r) {
          getDevices();
          dispatch(
            addNotification({
              title: `Reboot command sent to ${device.name}`,
              body: "",
              severity: "success",
            }),
          );
        } else {
          dispatch(
            addNotification({
              title: `Sending message failed`,
              body: "",
              severity: "error",
            }),
          );
        }
      })
      .catch((er) => {
        console.error(er);
        dispatch(
          addNotification({
            title: `Sending message failed`,
            body: "",
            severity: "error",
          }),
        );
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const addDevice = (model: AddOrUpdateDeviceDto) => {
    setIsLoading(true);
    deviceHook
      .addDevice(model)
      .then((res) => {
        dispatch(
          addNotification({
            title: `Device added for ${res.device.name}`,
            body: "",
            severity: "success",
          }),
        );
        getDevices();
      })
      .catch((er) => {
        console.error(er);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const sorted = [...deviceInfos].sort((a, b) =>
    dateTimeSort(a.lastMessage ?? new Date(), b.lastMessage ?? new Date()),
  );
  return (
    <AppContentWrapper
      title="Devices"
      isLoading={isLoading}
      titleComponent={
        <Box display="flex" alignItems="center" gap={1}>
          <Tooltip title="Add device">
            <IconButton
              onClick={() => setAddDeviceDialogOpen(true)}
              sx={{ ml: 1, cursor: "pointer" }}
              size="small"
              title="Add device"
            >
              <Add />
            </IconButton>
          </Tooltip>
        </Box>
      }
    >
      <DeviceTable
        devices={sorted}
        renderLink
        renderLinkToDeviceMessages
        hideId
        onReboot={(device) => {
          dispatch(
            setConfirmDialog({
              onConfirm: () => {
                rebootDevice(device.device);
              },
              title: `Reboot ${device.device.name}?`,
              body: `Reboot command will be sent to ${device.device.name}.  Id: ${device.device.identifier}, Identifier: '${device.device.identifier}'`,
            }),
          );
        }}
      />
      <EditDeviceDialog
        open={addDeviceDialogOpen}
        locations={locations}
        onClose={() => setAddDeviceDialogOpen(false)}
        onSave={addDevice}
      />
    </AppContentWrapper>
  );
};
