import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useEffect, useState } from "react";
import { type Device } from "../models/device";
import { useApiHook } from "../hooks/apiHook";
import { type DeviceInfo } from "../models/deviceInfo";
import { DeviceTable } from "../components/DeviceTable";
import { dateTimeSort } from "../utilities/datetimeUtils";
import { useDispatch } from "react-redux";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";

export const DevicesView: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const dispatch = useDispatch();
  const deviceHook = useApiHook().deviceHook;

  const [deviceInfos, setDeviceInfos] = useState<DeviceInfo[]>([]);

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
          setDeviceInfos(res);
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
