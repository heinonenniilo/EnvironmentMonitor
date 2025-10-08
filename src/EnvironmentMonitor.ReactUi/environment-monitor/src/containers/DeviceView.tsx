import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useEffect, useState } from "react";
import { useParams } from "react-router";
import { useApiHook } from "../hooks/apiHook";
import { type DeviceInfo } from "../models/deviceInfo";
import { SensorTable } from "../components/SensorTable";
import { DeviceTable } from "../components/DeviceTable";
import { DeviceControlComponent } from "../components/DeviceControlComponent";
import { useDispatch } from "react-redux";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";
import { DeviceEventTable } from "../components/DeviceEventTable";
import { type DeviceEvent } from "../models/deviceEvent";
import { Box } from "@mui/material";
import { DeviceImage } from "../components/DeviceImage";
import { Collapsible } from "../components/CollabsibleComponent";
import { MultiSensorGraph } from "../components/MultiSensorGraph";
import moment from "moment";
import { type MeasurementsViewModel } from "../models/measurementsBySensor";
import { type DeviceStatusModel } from "../models/deviceStatus";
import { MeasurementTypes } from "../enums/measurementTypes";
import { setDevices } from "../reducers/measurementReducer";
import { getDeviceTitle } from "../utilities/deviceUtils";
import { TimeRangeSelectorComponent } from "../components/TimeRangeSelectorComponent";
import { DeviceAttachments } from "../components/DeviceAttachments";

interface PromiseInfo {
  type: string;
  data: any | undefined;
}

const timeRangeDefaultDays = 7;

export const DeviceView: React.FC = () => {
  const [selectedDevice, setSelectedDevice] = useState<DeviceInfo | undefined>(
    undefined
  );
  const [deviceEvents, setDeviceEvents] = useState<DeviceEvent[]>([]);
  const [deviceStatusModel, setDeviceStatusModel] = useState<
    DeviceStatusModel | undefined
  >(undefined);

  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingMeasurements, setIsLoadingMeasurments] = useState(false);
  const [isLoadingDeviceStatus, setIsLoadingDeviceStatus] = useState(false);
  const [hasFetched, setHasFetched] = useState(false);
  const [model, setModel] = useState<MeasurementsViewModel | undefined>(
    undefined
  );

  const [statusTimeRange, setStatusTimeRange] = useState(
    timeRangeDefaultDays * 24
  );

  const [defaultImageVer, setDefaultImageVer] = useState(0);
  const dispatch = useDispatch();

  const { deviceId } = useParams<{ deviceId?: string }>();
  const deviceHook = useApiHook().deviceHook;
  const measurementApiHook = useApiHook().measureHook;

  const loadMeasurements = () => {
    if (!selectedDevice) {
      return;
    }

    const momentStart = moment()
      .local(true)
      .add(-1 * 48, "hour")
      .utc(true);

    setIsLoadingMeasurments(true);
    measurementApiHook
      .getMeasurementsBySensor(
        selectedDevice.sensors.map((x) => x.identifier),
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
        setIsLoadingMeasurments(false);
      });
  };
  useEffect(() => {
    if (!selectedDevice) {
      return;
    }
    const momentStart = moment()
      .local(true)
      .add(-1 * 48, "hour")
      .utc(true);
    setIsLoadingMeasurments(true);
    measurementApiHook
      .getMeasurementsBySensor(
        selectedDevice.sensors.map((x) => x.identifier),
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
        setIsLoadingMeasurments(false);
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedDevice]);

  useEffect(() => {
    if (!selectedDevice) {
      return;
    }
    const momentStart = moment()
      .local(true)
      .add(statusTimeRange ? -1 * statusTimeRange : -24, "hour")
      .utc(true);
    setIsLoadingDeviceStatus(true);
    deviceHook
      .getDeviceStatus(
        [selectedDevice.device.identifier],
        momentStart,
        undefined
      )
      .then((res) => {
        setDeviceStatusModel(res);
      })
      .catch((er) => {
        console.error(er);
      })
      .finally(() => {
        setIsLoadingDeviceStatus(false);
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedDevice, statusTimeRange]);

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

  const updateVisible = (device: DeviceInfo) => {
    setIsLoading(true);
    deviceHook
      .updateDevice({ ...device.device, visible: !device.device.visible })
      .then((res) => {
        setSelectedDevice(res);
        dispatch(
          addNotification({
            title: `Visibility status updated for ${res.device.name}`,
            body: "_",
            severity: "success",
          })
        );
        // Update devices
        measurementApiHook
          .getDevices()
          .then((res) => {
            if (res) {
              dispatch(setDevices(res));
            }
          })
          .catch((ex) => {
            console.error(ex);
          });
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
      .setMotionControlState(selectedDevice.device.identifier, state)
      .then((res) => {
        if (res) {
          getDeviceEvents(selectedDevice.device.identifier);
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
      .setMotionControlDelay(selectedDevice.device.identifier, delayMs)
      .then((res) => {
        if (res) {
          getDeviceEvents(selectedDevice.device.identifier);
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
      .rebootDevice(selectedDevice?.device.identifier)
      .then(() => {
        getDeviceEvents(selectedDevice?.device.identifier);
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

  const uploadAttachment = (
    file: File,
    isDeviceImage: boolean,
    name?: string
  ) => {
    if (selectedDevice === undefined) {
      return;
    }
    setIsLoading(true);

    deviceHook
      .uploadAttachment(
        selectedDevice?.device.identifier,
        file,
        isDeviceImage,
        name
      )
      .then((res) => {
        setSelectedDevice(res);
        if (isDeviceImage) {
          setDefaultImageVer(defaultImageVer + 1);
        }
        dispatch(
          addNotification({
            title: isDeviceImage ? "Image uploaded" : "Attachment uploaded",
            body: "",
            severity: "success",
          })
        );
      })
      .catch((ex) => {
        console.error("Failed to upload image.", ex);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const deleteAttachment = (identifier: string) => {
    if (selectedDevice === undefined) {
      return;
    }
    setIsLoading(true);

    deviceHook
      .deleteAttachment(selectedDevice.device.identifier, identifier)
      .then((res) => {
        setSelectedDevice(res);
        dispatch(
          addNotification({
            title: "Attachment deleted",
            body: "Success",
            severity: "success",
          })
        );
      })
      .catch((er) => {
        console.error(er);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const setDefaultImage = (identifier: string) => {
    if (!selectedDevice) {
      return;
    }

    setIsLoading(true);

    deviceHook
      .setDefaultImage(selectedDevice?.device.identifier, identifier)
      .then((res) => {
        setSelectedDevice(res);
        dispatch(
          addNotification({
            title: "Default image set",
            body: "Success",
            severity: "success",
          })
        );
      })
      .catch((ex) => {
        console.error(ex);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  return (
    <AppContentWrapper
      title={getDeviceTitle(selectedDevice?.device)}
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
            showDeviceIdentifier
            hideId
            onClickVisible={(device) => {
              dispatch(
                setConfirmDialog({
                  onConfirm: () => {
                    updateVisible(device);
                  },
                  title: "Update visible status",
                  body: `Visible status will be ${!device.device.visible} for ${
                    device.device.name
                  }`,
                })
              );
            }}
            devices={selectedDevice ? [selectedDevice] : []}
            disableSort
            renderLinkToDeviceMessages
          />
        </Collapsible>

        <DeviceImage
          device={selectedDevice}
          ver={defaultImageVer}
          title={"Device images"}
          onSetDefaultImage={(identifier: string) => {
            dispatch(
              setConfirmDialog({
                onConfirm: () => {
                  setDefaultImage(identifier);
                },
                title: "Set default image",
                body: `Current image will be set as default image for ${selectedDevice?.device.name}`,
              })
            );
          }}
          onDeleteImage={(identifier: string) => {
            dispatch(
              setConfirmDialog({
                onConfirm: () => {
                  deleteAttachment(identifier);
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
                  uploadAttachment(file, true);
                },
                title: "Upload new image?",
                body: `Upload "${file.name}"?`,
              })
            );
          }}
        />
        <Collapsible title="Sensors" isOpen={true}>
          <SensorTable sensors={selectedDevice?.sensors ?? []} />
        </Collapsible>

        <DeviceAttachments
          attachments={
            selectedDevice?.attachments.filter((a) => !a.isDeviceImage) ?? []
          }
          device={selectedDevice}
          onDeleteAttachment={(identifier: string) => {
            dispatch(
              setConfirmDialog({
                onConfirm: () => {
                  deleteAttachment(identifier);
                },
                title: "Delete attachment",
                body: `Attachment will be removed`,
              })
            );
          }}
          onUploadAttachment={(file, customName) => {
            uploadAttachment(file, false, customName);
          }}
        />

        <Collapsible title="Measurements">
          <MultiSensorGraph
            sensors={selectedDevice?.sensors}
            model={model}
            minHeight={400}
            title={`${selectedDevice?.device.name} - Last 48 h`}
            useAutoScale
            onRefresh={loadMeasurements}
            isLoading={isLoadingMeasurements}
          />
        </Collapsible>
        <Collapsible title="Online status">
          <TimeRangeSelectorComponent
            timeRange={statusTimeRange}
            onSelectTimeRange={setStatusTimeRange}
            options={[3 * 24, 7 * 24, 30 * 24, 90 * 24]}
            labels={["3 days", "7 days", "30 days", "90 days"]}
            selectedText={`${statusTimeRange / 24} days`}
          />
          <MultiSensorGraph
            title={`Last ${statusTimeRange / 24} days`}
            sensors={
              selectedDevice
                ? [
                    {
                      identifier: selectedDevice.device.identifier,
                      name: selectedDevice.device.name,
                      deviceIdentifier: selectedDevice.device.identifier,
                      scaleMin: 0,
                      scaleMax: 2,
                    },
                  ]
                : []
            }
            stepped
            zoomable
            hideInfo
            hideUseAutoScale
            highlightPoints
            minHeight={400}
            isLoading={isLoadingDeviceStatus}
            showMeasurementsOnDatasetClick
            model={
              selectedDevice
                ? {
                    measurements: [
                      {
                        sensorIdentifier: selectedDevice.device.identifier,
                        minValues: {},
                        maxValues: {},
                        latestValues: {},
                        measurements: deviceStatusModel
                          ? deviceStatusModel.deviceStatuses.map((d) => {
                              return {
                                sensorIdentifier:
                                  selectedDevice.device.identifier,
                                sensorValue: d.status ? 1 : 0,
                                typeId: MeasurementTypes.Online,
                                timestamp: d.timestamp,
                              };
                            })
                          : [],
                      },
                    ],
                  }
                : undefined
            }
          />
        </Collapsible>
        <Collapsible isOpen={true} title="Commands">
          <DeviceControlComponent
            device={selectedDevice}
            reboot={() => {
              dispatch(
                setConfirmDialog({
                  onConfirm: () => {
                    reboot(
                      `Boot command sent to ${selectedDevice?.device.name}`
                    );
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
        </Collapsible>
        <Collapsible title="Events" isOpen={true}>
          <DeviceEventTable events={deviceEvents} maxHeight={"500px"} />
        </Collapsible>
      </Box>
    </AppContentWrapper>
  );
};
