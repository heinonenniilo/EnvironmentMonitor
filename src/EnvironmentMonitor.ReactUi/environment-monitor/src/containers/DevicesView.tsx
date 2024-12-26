import { AppContentWrapper } from "../framework/AppContentWrapper";
import { ConfirmationDialog } from "../framework/ConfirmationDialog";
import { useEffect, useState } from "react";
import { Device } from "../models/device";
import { useApiHook } from "../hooks/apiHook";
import { DeviceInfo } from "../models/deviceInfo";
import { DeviceTable } from "../components/DeviceTable";
import { dateTimeSort } from "../utilities/datetimeUtils";
import { useDispatch } from "react-redux";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";

export const DevicesView: React.FC = () => {
  const [selectedDevice, setSelectedDevice] = useState<Device | undefined>(
    undefined
  );

  const [isLoading, setIsLoading] = useState(false);
  const dispatch = useDispatch();
  const deviceHook = useApiHook().deviceHook;

  const [deviceInfos, setDeviceInfos] = useState<DeviceInfo[]>([]);

  const getDialogTitle = () => {
    return `Reboot ${selectedDevice?.name}?`;
  };

  const getDialogBody = () => {
    if (!selectedDevice) {
      return "";
    }

    return `Name: ${selectedDevice.name}, Identifier: ${selectedDevice?.deviceIdentifier}, Id: ${selectedDevice.id}?`;
  };

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
    const deviceIdentifier = device.deviceIdentifier;

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
          alert("Sending the message failed!");
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
        alert("Sending message failed");
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
        setSelectedDevice(undefined);
      });
    //
  };
  /*
    const sorted = [...returnRows].sort((a, b) =>
      dateTimeSort(a.latest.timestamp, b.latest.timestamp)
    );
  */

  const sorted = [...deviceInfos].sort((a, b) =>
    dateTimeSort(a.lastMessage ?? new Date(), b.lastMessage ?? new Date())
  );
  return (
    <AppContentWrapper titleParts={[{ text: "Devices" }]} isLoading={isLoading}>
      <DeviceTable
        devices={sorted}
        showLink
        onReboot={(device) => {
          dispatch(
            setConfirmDialog({
              onConfirm: () => {
                rebootDevice(device.device);
              },
              title: `Reboot ${device.device.name}?`,
              body: `Reboot command will be sent to ${device.device.name}.  Id: ${device.device.id}, Identifier: '${device.device.deviceIdentifier}'`,
            })
          );
        }}
      />
    </AppContentWrapper>
  );
};
