import { AppContentWrapper } from "../framework/AppContentWrapper";

import { ConfirmationDialog } from "../framework/ConfirmationDialog";
import { useEffect, useState } from "react";
import { useParams } from "react-router";
import { useApiHook } from "../hooks/apiHook";
import { DeviceInfo } from "../models/deviceInfo";
import { SensorTable } from "../components/SensorTable";
import { DeviceTable } from "../components/DeviceTable";
import { DeviceControlComponent } from "../components/DeviceCommandButtons";

export const DeviceView: React.FC = () => {
  const [selectedDevice, setSelectedDevice] = useState<DeviceInfo | undefined>(
    undefined
  );

  const [motionControlStateToSet, setMotionControlStateToSet] = useState<
    number | undefined
  >(undefined);

  const [motionControlDelayToSet, setMotionControlDelayToSet] = useState<
    number | undefined
  >(undefined);

  const [dialogIsOpen, setDialogIsOpen] = useState(false);

  const [isLoading, setIsLoading] = useState(false);
  const [hasFetched, setHasFetched] = useState(false);

  const { deviceId } = useParams<{ deviceId?: string }>();
  const deviceHook = useApiHook().deviceHook;

  useEffect(() => {
    if (deviceId && !hasFetched) {
      setIsLoading(true);
      deviceHook
        .getDeviceInfo(deviceId)
        .then((res) => {
          console.log(res);
          setSelectedDevice(res);
        })
        .catch((er) => {})
        .finally(() => {
          setHasFetched(true);
          setIsLoading(false);
        });
    }
  }, [deviceId, hasFetched]);

  const setMotionControlState = (state: number) => {
    if (!selectedDevice) {
      return;
    }
    setIsLoading(true);
    deviceHook
      .setMotionControlState(selectedDevice.device.deviceIdentifier, state)
      .catch((er) => {
        console.error(er);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const setMotionControlDelay = (delay: number) => {
    if (!selectedDevice) {
      return;
    }
    setIsLoading(true);
    deviceHook
      .setMotionControlDelay(selectedDevice.device.deviceIdentifier, delay)
      .catch((er) => {
        console.error(er);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const sendCommand = () => {
    if (motionControlDelayToSet !== undefined) {
      setMotionControlDelay(motionControlDelayToSet);
    } else if (motionControlStateToSet !== undefined) {
      setMotionControlState(motionControlStateToSet);
    }
  };

  const getTitle = () => {
    if (motionControlStateToSet !== undefined) {
      return `Set motion control mode? (${motionControlStateToSet})`;
    } else if (motionControlDelayToSet !== undefined) {
      return `Set delay to ${motionControlDelayToSet} ms?`;
    }
    return "Confirm";
  };

  return (
    <AppContentWrapper
      titleParts={[{ text: `${selectedDevice?.device?.name ?? ""}` }]}
      isLoading={isLoading}
    >
      <ConfirmationDialog
        isOpen={dialogIsOpen}
        onClose={() => {
          setMotionControlDelayToSet(undefined);
          setMotionControlStateToSet(undefined);
          setDialogIsOpen(false);
        }}
        onConfirm={() => {
          sendCommand();
          setDialogIsOpen(false);
        }}
        title={getTitle()}
        body={"Confirm?"}
      />
      <DeviceTable
        title="Info"
        hideName
        devices={selectedDevice ? [selectedDevice] : []}
      />
      <SensorTable
        title="Sensors"
        sensors={selectedDevice?.device?.sensors ?? []}
      />
      <DeviceControlComponent
        device={selectedDevice}
        reboot={() => {}}
        onSetOutStatic={(mode: boolean) => {
          setMotionControlStateToSet(mode ? 1 : 0);
          setMotionControlDelayToSet(undefined);
          setDialogIsOpen(true);
        }}
        onSetOutOnMotionControl={() => {
          setMotionControlStateToSet(2);
          setMotionControlDelayToSet(undefined);
          setDialogIsOpen(true);
        }}
        onSetMotionControlDelay={(delay: number) => {
          setMotionControlStateToSet(undefined);
          setMotionControlDelayToSet(delay);
          setDialogIsOpen(true);
        }}
      />
    </AppContentWrapper>
  );
};
