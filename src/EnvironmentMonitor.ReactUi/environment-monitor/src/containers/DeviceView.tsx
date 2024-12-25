import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useEffect, useState } from "react";
import { useParams } from "react-router";
import { useApiHook } from "../hooks/apiHook";
import { DeviceInfo } from "../models/deviceInfo";
import { SensorTable } from "../components/SensorTable";
import { DeviceTable } from "../components/DeviceTable";
import { DeviceControlComponent } from "../components/DeviceCommandButtons";
import { useDispatch } from "react-redux";
import { setConfirmDialog } from "../reducers/userInterfaceReducer";

export const DeviceView: React.FC = () => {
  const [selectedDevice, setSelectedDevice] = useState<DeviceInfo | undefined>(
    undefined
  );

  const [isLoading, setIsLoading] = useState(false);
  const [hasFetched, setHasFetched] = useState(false);
  const dispatch = useDispatch();

  const { deviceId } = useParams<{ deviceId?: string }>();
  const deviceHook = useApiHook().deviceHook;

  useEffect(() => {
    if (deviceId && !hasFetched) {
      setIsLoading(true);
      deviceHook
        .getDeviceInfo(deviceId)
        .then((res) => {
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

  return (
    <AppContentWrapper
      titleParts={[{ text: `${selectedDevice?.device?.name ?? ""}` }]}
      isLoading={isLoading}
    >
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
          dispatch(
            setConfirmDialog({
              onConfirm: () => {
                setMotionControlState(mode ? 1 : 0);
              },
              title: `Set output as ${mode}`,
              body: `Output pins will be set as ${mode}. Motion sensor trigger will be disabled`,
            })
          );
        }}
        onSetOutOnMotionControl={() => {
          dispatch(
            setConfirmDialog({
              onConfirm: () => {
                setMotionControlState(2);
              },
              title: `Enable motion control`,
              body: "Output pins will be controlled by motion sensor",
            })
          );
        }}
        onSetMotionControlDelay={(delay: number) => {
          dispatch(
            setConfirmDialog({
              onConfirm: () => {
                setMotionControlDelay(delay);
              },
              title: `Set motion control delay`,
              body: `Motion control delay will be set to ${delay / 1000} s`,
            })
          );
        }}
      />
    </AppContentWrapper>
  );
};
