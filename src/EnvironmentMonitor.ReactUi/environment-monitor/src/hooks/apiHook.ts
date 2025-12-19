import qs from "qs";
import moment from "moment";
import { useDispatch } from "react-redux";
import { addNotification } from "../reducers/userInterfaceReducer";
import type { User } from "../models/user";
import type { LocationModel } from "../models/location";
import type { AxiosResponse } from "axios";
import type { Device } from "../models/device";
import type { DeviceEvent } from "../models/deviceEvent";
import type { DeviceInfo } from "../models/deviceInfo";
import type { DeviceStatusModel } from "../models/deviceStatus";
import type {
  MeasurementsModel,
  MeasurementsViewModel,
  MeasurementsByLocationModel,
} from "../models/measurementsBySensor";
import type { Sensor } from "../models/sensor";
import axios from "axios";
import type { GetDeviceMessagesModel } from "../models/getDeviceMessagesModel";
import type { PaginatedResult } from "../models/paginatedResult";
import type { DeviceMessage } from "../models/deviceMessage";
import type { GetMeasurementsModel } from "../models/getMeasurementsModel";
import type { DeviceAttribute } from "../models/deviceAttribute";
import type { GetQueuedCommandsModel } from "../models/getQueuedCommandsModel";
import type { DeviceQueuedCommandDto } from "../models/deviceQueuedCommand";
import type { DeviceContact } from "../models/deviceContact";
import type { AddOrUpdateDeviceContact } from "../models/addOrUpdateDeviceContact";

interface ApiHook {
  userHook: userHook;
  measureHook: measureHook;
  deviceHook: deviceHook;
  locationHook: locationHook;
  deviceContactsHook: deviceContactsHook;
}

interface userHook {
  getUserInfo: () => Promise<User | undefined>;
  logIn: (
    userId: string,
    password: string,
    persistent: boolean
  ) => Promise<boolean>;
  logOut: () => Promise<boolean>;
}

interface locationHook {
  getLocations: () => Promise<LocationModel[]>;
}

interface measureHook {
  getDevices: () => Promise<Device[] | undefined>;
  getSensors: (deviceId: string[]) => Promise<Sensor[]>;
  getMeasurements: (
    model: GetMeasurementsModel
  ) => Promise<MeasurementsModel | undefined>;
  getMeasurementsBySensor: (
    sensorIds: string[],
    from: moment.Moment,
    to?: moment.Moment,
    latestOnly?: boolean,
    deviceIds?: string[]
  ) => Promise<MeasurementsViewModel | undefined>;
  getMeasurementsByLocation: (
    locationIds: string[],
    from: moment.Moment,
    to?: moment.Moment,
    latestOnly?: boolean,
    sensorIds?: string[]
  ) => Promise<MeasurementsByLocationModel | undefined>;

  getPublicMeasurements: (
    from: moment.Moment,
    to?: moment.Moment,
    latestOnly?: boolean
  ) => Promise<MeasurementsViewModel | undefined>;
}

interface deviceHook {
  rebootDevice: (deviceIdentifier: string) => Promise<boolean>;
  getDeviceInfos: () => Promise<DeviceInfo[] | undefined>;
  getDeviceInfo: (identifier: string) => Promise<DeviceInfo>;
  setMotionControlState: (
    identifier: string,
    state: number,
    executeAt?: moment.Moment
  ) => Promise<DeviceAttribute[]>;
  setMotionControlDelay: (
    identifier: string,
    delayMs: number,
    executeAt?: moment.Moment
  ) => Promise<DeviceAttribute[]>;
  getDeviceEvents: (identifier: string) => Promise<DeviceEvent[]>;
  uploadAttachment: (
    identifire: string,
    file: File,
    isDeviceImage: boolean,
    fileName?: string,
    isSecret?: boolean
  ) => Promise<DeviceInfo | undefined>;
  deleteAttachment: (
    deviceIdentifier: string,
    attachmentIdentifier: string
  ) => Promise<DeviceInfo | undefined>;
  setDefaultImage: (
    deviceIdentifier: string,
    attachmentIdentifier: string
  ) => Promise<DeviceInfo | undefined>;
  getDeviceStatus: (
    deviceIdentifiers: string[],
    from: moment.Moment,
    to?: moment.Moment
  ) => Promise<DeviceStatusModel>;
  updateDevice: (device: Device) => Promise<DeviceInfo>;
  getDeviceMessage: (
    model: GetDeviceMessagesModel
  ) => Promise<PaginatedResult<DeviceMessage>>;
  getQueuedCommands: (
    model: GetQueuedCommandsModel
  ) => Promise<DeviceQueuedCommandDto[]>;
  deleteQueuedCommand: (
    deviceIdentifier: string,
    messageId: string
  ) => Promise<boolean>;
  updateQueuedCommand: (
    deviceIdentifier: string,
    messageId: string,
    newScheduledTime: moment.Moment
  ) => Promise<boolean>;
}

interface deviceContactsHook {
  create: (model: AddOrUpdateDeviceContact) => Promise<DeviceContact>;
  update: (model: AddOrUpdateDeviceContact) => Promise<DeviceContact>;
  delete: (model: AddOrUpdateDeviceContact) => Promise<void>;
}

const apiClient = axios.create({
  baseURL: undefined,
  withCredentials: true,
  paramsSerializer: (params) => {
    return qs.stringify(params, { arrayFormat: "repeat" }); // or "comma" if backend expects "1,2,3"
  },
});

export const useApiHook = (): ApiHook => {
  const dispatch = useDispatch();

  const showError = (title?: string, body?: string) => {
    dispatch(
      addNotification({
        title: title ?? "Error occured",
        body: body ?? "",
        severity: "error",
      })
    );
  };
  return {
    userHook: {
      getUserInfo: async () => {
        try {
          const response = await apiClient.get<
            any,
            AxiosResponse<User | undefined>
          >("/api/authentication/info");
          if (!response.data) {
            return undefined;
          }
          return response.data;
        } catch (ex: any) {
          console.error(ex);
          showError();
          return undefined;
        }
      },
      logIn: async (userId, password, persistent) => {
        try {
          const response = await apiClient.post<any, AxiosResponse<boolean>>(
            "/api/authentication/login",
            {
              userName: userId,
              password: password,
              persistent: persistent,
            }
          );
          return response.data;
        } catch (ex: any) {
          console.error(ex);
          showError("Login failed");
          return false;
        }
      },
      logOut: async () => {
        try {
          await apiClient.post<any, AxiosResponse<User>>(
            "/api/authentication/logout"
          );
          return true;
        } catch (ex: any) {
          console.error(ex);
          showError();
          return false;
        }
      },
    },
    measureHook: {
      getDevices: async () => {
        try {
          const res = await apiClient.get<any, AxiosResponse<Device[]>>(
            "/api/devices"
          );
          return res.data;
        } catch (ex: any) {
          console.error(ex);
          showError();
          return undefined;
        }
      },
      getSensors: async (deviceIds: string[]) => {
        try {
          const res = await apiClient.get<any, AxiosResponse<Sensor[]>>(
            `/api/devices/sensors/`,
            {
              params: {
                deviceIds: deviceIds,
              },
            }
          );
          return res.data;
        } catch (ex: any) {
          console.error(ex);
          showError();
          return [];
        }
      },
      getMeasurements: async (model: GetMeasurementsModel) => {
        try {
          const res = await apiClient.get<
            any,
            AxiosResponse<MeasurementsModel>
          >("/api/Measurements", {
            params: {
              ...model,
              from: model.from ? model.from.toISOString() : undefined,
              to: model.to ? model.to.toISOString() : undefined,
            },
          });
          return res.data;
        } catch (ex: any) {
          console.error(ex);
          showError("Fetching measurements failed");
          return undefined;
        }
      },
      getMeasurementsBySensor: async (
        sensorIds: string[],
        from: moment.Moment,
        to?: moment.Moment,
        latestOnly?: boolean,
        deviceIds?: string[]
      ) => {
        try {
          const res = await apiClient.get<
            any,
            AxiosResponse<MeasurementsViewModel>
          >("/api/Measurements/bysensor", {
            params: {
              SensorIdentifiers: sensorIds,
              DeviceIdentifiers: deviceIds,
              from: from.toISOString(),
              to: to ? to.toISOString() : undefined,
              latestOnly: latestOnly,
            },
          });
          return res.data;
        } catch (ex: any) {
          console.error(ex);
          showError("Fetching measurements failed");
          return undefined;
        }
      },
      getMeasurementsByLocation: async (
        locationIds: string[],
        from: moment.Moment,
        to?: moment.Moment,
        latestOnly?: boolean,
        sensorIds?: string[]
      ) => {
        try {
          const res = await apiClient.get<
            any,
            AxiosResponse<MeasurementsByLocationModel>
          >("/api/Measurements/bylocation", {
            params: {
              locationIdentifiers: locationIds,
              from: from.toISOString(),
              to: to ? to.toISOString() : undefined,
              latestOnly: latestOnly,
              sensorIdentifiers: sensorIds,
            },
          });
          return res.data;
        } catch (ex: any) {
          console.error(ex);
          showError("Fetching measurements failed");
          return undefined;
        }
      },
      getPublicMeasurements: async (
        from: moment.Moment,
        to?: moment.Moment,
        latestOnly?: boolean
      ) => {
        try {
          const res = await apiClient.get<
            any,
            AxiosResponse<MeasurementsViewModel>
          >("/api/Measurements/public", {
            params: {
              SensorIds: [],
              from: from.toISOString(),
              to: to ? to.toISOString() : undefined,
              latestOnly: latestOnly,
            },
          });
          return res.data;
        } catch (ex: any) {
          console.error(ex);
          showError("Fetching measurements failed");
          return undefined;
        }
      },
    },
    deviceHook: {
      updateDevice: async (device: Device) => {
        try {
          const res = await apiClient.put("/api/devices/update", {
            device: device,
          });

          return res.data;
        } catch (ex: any) {
          console.error(ex);
          showError("Failed to update device");
          throw ex;
        }
      },
      rebootDevice: async (deviceIdentifier: string) => {
        try {
          const res = await apiClient.post("/api/devices/reboot", {
            deviceIdentifier: deviceIdentifier,
          });

          if (res.status === 200) {
            return true;
          } else {
            return false;
          }
        } catch (ex: any) {
          console.error(ex);
          showError();
          return false;
        }
      },
      getDeviceInfos: async () => {
        try {
          const res = await apiClient.get<any, AxiosResponse<DeviceInfo[]>>(
            "/api/devices/info"
          );
          return res.data;
        } catch (ex: any) {
          console.error(ex);
          showError();
          return undefined;
        }
      },
      getDeviceInfo: async (identifier: string) => {
        const res = await apiClient.get<any, AxiosResponse<DeviceInfo>>(
          `/api/devices/${identifier}/info`
        );
        return res.data;
      },
      setMotionControlState: async (
        identifier: string,
        state: number,
        executeAt?: moment.Moment
      ) => {
        try {
          const res = await apiClient.post<
            any,
            AxiosResponse<DeviceAttribute[]>
          >("/api/devices/motion-control-status", {
            deviceIdentifier: identifier,
            mode: state,
            executeAt: executeAt
              ? executeAt.format("YYYY-MM-DDTHH:mm:ss")
              : undefined,
          });
          return res.data;
        } catch (ex) {
          console.error(ex);
          showError();
          return [];
        }
      },
      setMotionControlDelay: async (
        identifier: string,
        delayMs: number,
        executeAt?: moment.Moment
      ) => {
        try {
          const res = await apiClient.post<
            any,
            AxiosResponse<DeviceAttribute[]>
          >("/api/devices/motion-control-delay", {
            deviceIdentifier: identifier,
            DelayMs: delayMs,
            executeAt: executeAt
              ? executeAt.format("YYYY-MM-DDTHH:mm:ss")
              : undefined,
          });
          return res.data;
        } catch (ex) {
          console.error(ex);
          showError();
          return [];
        }
      },
      getDeviceEvents: async (identifier: string) => {
        try {
          const res = await apiClient.get<any, AxiosResponse<DeviceEvent[]>>(
            `/api/devices/${identifier}/events`
          );
          return res.data;
        } catch (ex) {
          console.error(ex);
          showError();
          return [];
        }
      },
      uploadAttachment: async (
        identifier: string,
        file: File,
        isDeviceImage: boolean,
        fileName?: string,
        isSecret?: boolean
      ) => {
        const formData = new FormData();
        formData.append("file", file);
        formData.append("deviceId", identifier);
        formData.append("isDeviceImage", isDeviceImage ? "true" : "false");
        formData.append("isSecret", isSecret ? "true" : "false");
        if (fileName) {
          formData.append("fileName", fileName);
        }
        try {
          const res = await apiClient.post<any, AxiosResponse<DeviceInfo>>(
            `/api/devices/attachment/`,
            formData
          );
          return res.data;
        } catch (ex) {
          showError("Failed to upload image");
          throw ex;
        }
      },
      deleteAttachment: async (
        deviceIdentifier: string,
        attachmentIdentifier: string
      ) => {
        try {
          const res = await apiClient.delete<any, AxiosResponse<DeviceInfo>>(
            `/api/devices/${deviceIdentifier}/attachment/${attachmentIdentifier}`
          );
          return res.data;
        } catch (ex) {
          showError("Deleting attachment failed");
          throw ex;
        }
      },
      setDefaultImage: async (
        deviceIdentifier: string,
        attachmentIdentifier: string
      ) => {
        try {
          const res = await apiClient.post<any, AxiosResponse<DeviceInfo>>(
            `/api/devices/default-image`,
            {
              attachmentGuid: attachmentIdentifier,
              deviceIdentifier: deviceIdentifier,
            }
          );
          return res.data;
        } catch (ex) {
          showError("Setting default attachment failed");
          throw ex;
        }
      },
      getDeviceStatus: async (
        deviceIdentifiers: string[],
        from: moment.Moment,
        to?: moment.Moment
      ) => {
        try {
          const res = await apiClient.get<
            any,
            AxiosResponse<DeviceStatusModel>
          >(`/api/devices/status`, {
            params: {
              deviceIdentifiers: deviceIdentifiers,
              from: from.toISOString(),
              to: to ? to.toISOString() : undefined,
            },
          });
          return res.data;
        } catch (ex) {
          showError("Failed to fetch device status");
          throw ex;
        }
      },
      getDeviceMessage: async (model: GetDeviceMessagesModel) => {
        try {
          const res = await apiClient.get<
            any,
            AxiosResponse<PaginatedResult<DeviceMessage>>
          >(`/api/devices/device-messages`, {
            params: {
              ...model,
              from: model.from.toISOString(),
              to: model.to ? model.to.toISOString() : undefined,
            },
          });
          return res.data;
        } catch (ex) {
          console.error(ex);
          showError("Failed to get device messages");
          throw ex;
        }
      },
      getQueuedCommands: async (model: GetQueuedCommandsModel) => {
        try {
          const res = await apiClient.get<
            any,
            AxiosResponse<DeviceQueuedCommandDto[]>
          >(`/api/devices/queued-commands`, {
            params: model,
          });
          return res.data;
        } catch (ex) {
          console.error(ex);
          showError("Failed to get queued commands");
          throw ex;
        }
      },
      deleteQueuedCommand: async (
        deviceIdentifier: string,
        messageId: string
      ) => {
        try {
          const res = await apiClient.delete<any, AxiosResponse<boolean>>(
            `/api/devices/${deviceIdentifier}/queued-commands/${messageId}`
          );
          return res.status === 200;
        } catch (ex) {
          console.error(ex);
          showError("Failed to delete queued command");
          throw ex;
        }
      },
      updateQueuedCommand: async (
        deviceIdentifier: string,
        messageId: string,
        newScheduledTime: moment.Moment
      ) => {
        try {
          const res = await apiClient.put<any, AxiosResponse<boolean>>(
            `/api/devices/queued-commands/schedule`,
            {
              deviceIdentifier: deviceIdentifier,
              messageId: messageId,
              newScheduledTime: newScheduledTime.format("YYYY-MM-DDTHH:mm:ss"),
            }
          );
          return res.status === 200;
        } catch (ex) {
          console.error(ex);
          showError("Failed to update queued command");
          throw ex;
        }
      },
    },
    locationHook: {
      getLocations: async () => {
        try {
          const res = await apiClient.get<any, AxiosResponse<LocationModel[]>>(
            `/api/locations/`
          );
          return res.data;
        } catch (ex) {
          console.error(ex);
          showError("Failed to get locations");
          return [];
        }
      },
    },
    deviceContactsHook: {
      create: async (model: AddOrUpdateDeviceContact) => {
        try {
          const res = await apiClient.post<any, AxiosResponse<DeviceContact>>(
            `/api/devicecontacts`,
            model
          );
          return res.data;
        } catch (ex) {
          console.error(ex);
          showError("Failed to create device contact");
          throw ex;
        }
      },
      update: async (model: AddOrUpdateDeviceContact) => {
        try {
          const res = await apiClient.put<any, AxiosResponse<DeviceContact>>(
            `/api/devicecontacts`,
            model
          );
          return res.data;
        } catch (ex) {
          console.error(ex);
          showError("Failed to update device contact");
          throw ex;
        }
      },
      delete: async (model: AddOrUpdateDeviceContact) => {
        try {
          await apiClient.delete(`/api/devicecontacts`, { data: model });
        } catch (ex) {
          console.error(ex);
          showError("Failed to delete device contact");
          throw ex;
        }
      },
    },
  };
};
