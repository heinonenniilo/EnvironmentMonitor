import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useEffect, useState } from "react";
import { useParams } from "react-router";
import { useApiHook } from "../hooks/apiHook";
import { DeviceInfo } from "../models/deviceInfo";
import { SensorTable } from "../components/SensorTable";
import { DeviceTable } from "../components/DeviceTable";
import { DeviceControlComponent } from "../components/DeviceControlComponent";
import { useDispatch } from "react-redux";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";
import { DeviceEventTable } from "../components/DeviceEventTable";
import { DeviceEvent } from "../models/deviceEvent";
import { Box } from "@mui/material";

interface PromiseInfo {
  type: string;
  data: any | undefined;
}

export const DeviceView: React.FC = () => {
  const [selectedDevice, setSelectedDevice] = useState<DeviceInfo | undefined>(
    undefined
  );
  const [deviceEvents, setDeviceEvents] = useState<DeviceEvent[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const [hasFetched, setHasFetched] = useState(false);
  const dispatch = useDispatch();

  const { deviceId } = useParams<{ deviceId?: string }>();
  const deviceHook = useApiHook().deviceHook;

  useEffect(() => {
    if (deviceId && !hasFetched) {
      const promises = [
        deviceHook
          .getDeviceInfo(deviceId)
          .then((res) => ({
            type: "deviceInfo",
            data: res,
            error: null,
          }))
          .catch((error) => ({
            type: "deviceInfo",
            data: null,
            error,
          })),
        deviceHook
          .getDeviceEvents(deviceId)
          .then((res) => ({
            type: "deviceEvents",
            data: res,
            error: null,
          }))
          .catch((er) => {
            console.log(er);
          }),
      ];

      setIsLoading(true);
      Promise.allSettled(promises)
        .then((results) => {
          results.forEach((result) => {
            if (result.status === "fulfilled") {
              const value = result.value as PromiseInfo;
              if (value.type === "deviceInfo") {
                setSelectedDevice(value.data as DeviceInfo);
              } else if (value.type === "deviceEvents") {
                setDeviceEvents(value.data as DeviceEvent[]);
              }
            } else if (result.status === "rejected") {
              console.error(
                `Error in ${result.reason.type}:`,
                result.reason.error
              );
            }
          });
        })
        .finally(() => {
          setHasFetched(true);
          setIsLoading(false);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [deviceId, hasFetched]);

  const getDeviceEvents = (id: string) => {
    setIsLoading(true);
    deviceHook
      .getDeviceEvents(id)
      .then((res) => {
        setDeviceEvents(res);
      })
      .catch((er) => {
        console.error(er);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const setMotionControlState = (state: number, message?: string) => {
    if (!selectedDevice) {
      return;
    }
    setIsLoading(true);
    deviceHook
      .setMotionControlState(selectedDevice.device.deviceIdentifier, state)
      .then((res) => {
        if (res) {
          getDeviceEvents(selectedDevice.device.deviceIdentifier);
          dispatch(
            addNotification({
              title: message ?? "Message sent to device",
              body: "",
              id: 0,
              severity: "success",
            })
          );
        }
      })
      .catch((er) => {
        console.error(er);
        setIsLoading(false);
        dispatch(
          addNotification({
            title: "Failed to send message",
            body: "",
            severity: "error",
          })
        );
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const setMotionControlDelay = (delayMs: number, message?: string) => {
    if (!selectedDevice) {
      return;
    }
    setIsLoading(true);
    deviceHook
      .setMotionControlDelay(selectedDevice.device.deviceIdentifier, delayMs)
      .then((res) => {
        if (res) {
          getDeviceEvents(selectedDevice.device.deviceIdentifier);
          dispatch(
            addNotification({
              title: message ?? "Message sent to device",
              body: "",
              severity: "success",
            })
          );
        }
      })
      .catch((er) => {
        console.error(er);
        dispatch(
          addNotification({
            title: "Sending message failed",
            body: "",
            severity: "error",
          })
        );
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const reboot = (message?: string) => {
    if (!selectedDevice) {
      return;
    }
    setIsLoading(true);
    deviceHook
      .rebootDevice(selectedDevice?.device.deviceIdentifier)
      .then((res) => {
        getDeviceEvents(selectedDevice?.device.deviceIdentifier);
        dispatch(
          addNotification({
            title: message ?? "Message sent to device",
            body: "",
            severity: "success",
          })
        );
      })
      .catch((er) => {
        console.error(er);
        dispatch(
          addNotification({
            title: "Failed to send the message",
            body: "",
            severity: "error",
          })
        );
        setIsLoading(false);
      });
  };

  return (
    <AppContentWrapper
      title={`${selectedDevice?.device?.name ?? ""}`}
      isLoading={isLoading}
    >
      <Box
        display={"flex"}
        flexGrow={"1"}
        flexDirection={"column"}
        height={"100%"}
      >
        <DeviceTable
          title="Info"
          hideName
          devices={selectedDevice ? [selectedDevice] : []}
          disableSort
        />
        <SensorTable
          title="Sensors"
          sensors={selectedDevice?.device?.sensors ?? []}
        />
        <DeviceControlComponent
          device={selectedDevice}
          title="Commands"
          reboot={() => {
            dispatch(
              setConfirmDialog({
                onConfirm: () => {
                  reboot(`Boot command sent to ${selectedDevice?.device.name}`);
                },
                title: `Reboot device?`,
                body: `${selectedDevice?.device.name} will be rebooted`,
              })
            );
          }}
          onSetOutStatic={(mode: boolean) => {
            dispatch(
              setConfirmDialog({
                onConfirm: () => {
                  setMotionControlState(
                    mode ? 1 : 0,
                    `Outputs set to ${mode} for ${selectedDevice?.device.name}`
                  );
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
                  setMotionControlState(2, "Motion control enabled");
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
                  setMotionControlDelay(
                    delay * 1000,
                    `Motioncontrol delay set to ${delay} s`
                  );
                },
                title: `Set motion control delay`,
                body: `Motion control delay will be set to ${delay} s`,
              })
            );
          }}
        />
        <DeviceEventTable
          events={deviceEvents}
          title="Events"
          maxHeight={"500px"}
        />
      </Box>
    </AppContentWrapper>
  );
};
