import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useEffect, useState } from "react";
import { type Device } from "../models/device";
import { useApiHook } from "../hooks/apiHook";
import { DeviceTable } from "../components/DeviceTable";
import { dateTimeSort } from "../utilities/datetimeUtils";
import { useDispatch, useSelector } from "react-redux";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";
import { getDeviceInfos, setDeviceInfos } from "../reducers/measurementReducer";

export const DevicesView: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const dispatch = useDispatch();
  const deviceInfos = useSelector(getDeviceInfos);
  const deviceHook = useApiHook().deviceHook;

  useEffect(() => {
    if (deviceInfos.length === 0 && deviceHook) {
      getDevices();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [deviceInfos]);

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
            })
          );
        } else {
          dispatch(
            addNotification({
              title: `Sending message failed`,
              body: "",
              severity: "error",
            })
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
          })
        );
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const sorted = [...deviceInfos].sort((a, b) =>
    dateTimeSort(a.lastMessage ?? new Date(), b.lastMessage ?? new Date())
  );
  return (
    <AppContentWrapper title="Devices" isLoading={isLoading}>
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
            })
          );
        }}
      />
    </AppContentWrapper>
  );
};
