import { AppContentWrapper } from "../framework/AppContentWrapper";
import { ConfirmationDialog } from "../framework/ConfirmationDialog";
import { useEffect, useState } from "react";
import { Device } from "../models/device";
import { useApiHook } from "../hooks/apiHook";
import { DeviceInfo } from "../models/deviceInfo";
import { DeviceTable } from "../components/DeviceTable";
import { dateTimeSort } from "../utilities/datetimeUtils";

export const DevicesView: React.FC = () => {
  const [selectedDevice, setSelectedDevice] = useState<Device | undefined>(
    undefined
  );

  const [isLoading, setIsLoading] = useState(false);
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

  const rebootDevice = () => {
    if (!selectedDevice) {
      return;
    }
    setIsLoading(true);
    const deviceIdentifier = selectedDevice.deviceIdentifier;
    setSelectedDevice(undefined);

    deviceHook
      .rebootDevice(deviceIdentifier)
      .then((r) => {
        if (r) {
          alert("Message sent to device");
          getDevices();
        } else {
          alert("Sending the message failed!");
        }
      })
      .catch((er) => {
        alert("Sending message failed");
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
      <ConfirmationDialog
        isOpen={selectedDevice !== undefined}
        onClose={() => {
          setSelectedDevice(undefined);
        }}
        onConfirm={rebootDevice}
        title={getDialogTitle()}
        body={getDialogBody()}
      />
      <DeviceTable
        devices={sorted}
        showLink
        onReboot={(device) => {
          setSelectedDevice(device.device);
        }}
      />
    </AppContentWrapper>
  );
};
