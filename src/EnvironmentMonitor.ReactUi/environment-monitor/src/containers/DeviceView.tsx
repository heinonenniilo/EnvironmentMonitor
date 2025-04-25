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
import { DeviceImage } from "../components/DeviceImage";
import { Collapsible } from "../components/CollabsibleComponent";
import { MultiSensorGraph } from "../components/MultiSensorGraph";
import moment from "moment";
import { MeasurementsViewModel } from "../models/measurementsBySensor";

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
  const [model, setModel] = useState<MeasurementsViewModel | undefined>(
    undefined
  );
  const [defaultImageVer, setDefaultImageVer] = useState(0);
  const dispatch = useDispatch();

  const { deviceId } = useParams<{ deviceId?: string }>();
  const deviceHook = useApiHook().deviceHook;
  const measurementApiHook = useApiHook().measureHook;

  useEffect(() => {
    if (!selectedDevice) {
      return;
    }
    const momentStart = moment()
      .local(true)
      .add(-1 * 48, "hour")
      .utc(true);
    setIsLoading(true);
    measurementApiHook
      .getMeasurementsBySensor(
        selectedDevice.device.sensors.map((x) => x.id),
        momentStart,
        undefined
      )
      .then((res) => {
        setModel(res);
      })
      .catch((er) => {
        console.error(er);
      })
      .finally(() => {
        setIsLoading(false);
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedDevice]);

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

  const uploadImage = (file: File) => {
    if (selectedDevice === undefined) {
      return;
    }
    //
    setIsLoading(true);

    deviceHook
      .uploadImage(selectedDevice?.device.deviceIdentifier, file)
      .then((res) => {
        setSelectedDevice(res);
        setDefaultImageVer(defaultImageVer + 1);
        dispatch(
          addNotification({
            title: "Image uploaded",
            body: "",
            severity: "success",
          })
        );
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const deleteImage = (identifier: string) => {
    if (selectedDevice === undefined) {
      return;
    }
    setIsLoading(true);

    deviceHook
      .deleteAttachment(selectedDevice.device.deviceIdentifier, identifier)
      .then((res) => {
        setSelectedDevice(res);
        dispatch(
          addNotification({
            title: "Image deleted",
            body: "Success",
            severity: "success",
          })
        );
      })
      .catch((er) => {
        //
      })
      .finally(() => {
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
        <Collapsible title="Info" isOpen={true}>
          <DeviceTable
            hideName
            devices={selectedDevice ? [selectedDevice] : []}
            disableSort
          />
        </Collapsible>

        <DeviceImage
          device={selectedDevice}
          ver={defaultImageVer}
          title={"Device images"}
          onDeleteImage={(identifier: string) => {
            dispatch(
              setConfirmDialog({
                onConfirm: () => {
                  deleteImage(identifier);
                },
                title: "Delete image",
                body: `The selected image of ${selectedDevice?.device.name} will be removed.`,
              })
            );
          }}
          onUploadImage={(file) => {
            dispatch(
              setConfirmDialog({
                onConfirm: () => {
                  uploadImage(file);
                },
                title: "Upload new image?",
                body: `Upload "${file.name}"?`,
              })
            );
          }}
        />
        <Collapsible title="Sensors" isOpen={true}>
          <SensorTable sensors={selectedDevice?.device?.sensors ?? []} />
        </Collapsible>

        <Collapsible title="Measurements">
          <MultiSensorGraph
            sensors={selectedDevice?.device.sensors}
            model={model}
            minHeight={400}
            title={`${selectedDevice?.device.name} - Last 48 h`}
            useAutoScale
          />
        </Collapsible>
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
        <Collapsible title="Events" isOpen={true}>
          <DeviceEventTable events={deviceEvents} maxHeight={"500px"} />
        </Collapsible>
      </Box>
    </AppContentWrapper>
  );
};
