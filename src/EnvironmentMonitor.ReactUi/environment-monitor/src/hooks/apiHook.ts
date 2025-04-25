import axios, { AxiosResponse } from "axios";
import { User } from "../models/user";
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";
import qs from "qs";
import {
  MeasurementsByLocationModel,
  MeasurementsModel,
  MeasurementsViewModel,
} from "../models/measurementsBySensor";
import moment from "moment";
import { DeviceInfo } from "../models/deviceInfo";
import { DeviceEvent } from "../models/deviceEvent";
import { useDispatch } from "react-redux";
import { addNotification } from "../reducers/userInterfaceReducer";
import { LocationModel } from "../models/location";

interface ApiHook {
  userHook: userHook;
  measureHook: measureHook;
  deviceHook: deviceHook;
  locationHook: locationHook;
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
    sensorIds: number[],
    from: moment.Moment,
    to?: moment.Moment
  ) => Promise<MeasurementsModel | undefined>;
  getMeasurementsBySensor: (
    sensorIds: number[],
    from: moment.Moment,
    to?: moment.Moment,
    latestOnly?: boolean
  ) => Promise<MeasurementsViewModel | undefined>;
  getMeasurementsByLocation: (
    sensorIds: number[],
    from: moment.Moment,
    to?: moment.Moment,
    latestOnly?: boolean
  ) => Promise<MeasurementsByLocationModel | undefined>;
}

interface deviceHook {
  rebootDevice: (deviceIdentifier: string) => Promise<boolean>;
  getDeviceInfos: () => Promise<DeviceInfo[] | undefined>;
  getDeviceInfo: (identifier: string) => Promise<DeviceInfo>;
  setMotionControlState: (
    identifier: string,
    state: number
  ) => Promise<boolean>;
  setMotionControlDelay: (
    identifier: string,
    delayMs: number
  ) => Promise<boolean>;
  getDeviceEvents: (identifier: string) => Promise<DeviceEvent[]>;
  uploadImage: (
    identifire: string,
    file: File
  ) => Promise<DeviceInfo | undefined>;
  deleteAttachment: (
    deviceIdentifier: string,
    attachmentIdentifier: string
  ) => Promise<DeviceInfo | undefined>;
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
          let res = await apiClient.get<any, AxiosResponse<Device[]>>(
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
          let res = await apiClient.get<any, AxiosResponse<Sensor[]>>(
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
      getMeasurements: async (
        sensorIds: number[],
        from: moment.Moment,
        to?: moment.Moment
      ) => {
        try {
          let res = await apiClient.get<any, AxiosResponse<MeasurementsModel>>(
            "/api/Measurements",
            {
              params: {
                SensorIds: sensorIds,
                from: from.toISOString(),
                to: to ? to.toISOString() : undefined,
              },
            }
          );
          return res.data;
        } catch (ex: any) {
          console.error(ex);
          showError("Fetching measurements failed");
          return undefined;
        }
      },
      getMeasurementsBySensor: async (
        sensorIds: number[],
        from: moment.Moment,
        to?: moment.Moment,
        latestOnly?: boolean
      ) => {
        try {
          let res = await apiClient.get<
            any,
            AxiosResponse<MeasurementsViewModel>
          >("/api/Measurements/bysensor", {
            params: {
              SensorIds: sensorIds,
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
        sensorIds: number[],
        from: moment.Moment,
        to?: moment.Moment,
        latestOnly?: boolean
      ) => {
        try {
          console.log(sensorIds);
          let res = await apiClient.get<
            any,
            AxiosResponse<MeasurementsByLocationModel>
          >("/api/Measurements/bylocation", {
            params: {
              SensorIds: sensorIds,
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
      rebootDevice: async (deviceIdentifier: string) => {
        try {
          let res = await apiClient.post("/api/devices/reboot", {
            deviceIdentifier: deviceIdentifier,
          });

          if (res.status === 200) {
            return true;
          } else {
            return false;
          }
        } catch (ex: any) {
          showError();
          return false;
        }
      },
      getDeviceInfos: async () => {
        try {
          let res = await apiClient.get<any, AxiosResponse<DeviceInfo[]>>(
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
        let res = await apiClient.get<any, AxiosResponse<DeviceInfo>>(
          `/api/devices/info/${identifier}`
        );
        return res.data;
      },
      setMotionControlState: async (identifier: string, state: number) => {
        try {
          let res = await apiClient.post("/api/devices/motion-control-status", {
            deviceIdentifier: identifier,
            mode: state,
          });
          if (res.status === 200) {
            return true;
          } else {
            return false;
          }
        } catch (ex) {
          showError();
          return false;
        }
      },
      setMotionControlDelay: async (identifier: string, delayMs: number) => {
        try {
          let res = await apiClient.post("/api/devices/motion-control-delay", {
            deviceIdentifier: identifier,
            DelayMs: delayMs,
          });
          if (res.status === 200) {
            return true;
          } else {
            return false;
          }
        } catch (ex) {
          showError();
          return false;
        }
      },
      getDeviceEvents: async (identifier: string) => {
        try {
          let res = await apiClient.get<any, AxiosResponse<DeviceEvent[]>>(
            `/api/devices/events/${identifier}`
          );
          return res.data;
        } catch (ex) {
          showError();
          return [];
        }
      },
      uploadImage: async (identifier: string, file: File) => {
        const formData = new FormData();
        formData.append("file", file);
        formData.append("deviceId", identifier);
        try {
          let res = await apiClient.post<any, AxiosResponse<DeviceInfo>>(
            `/api/devices/attachment/`,
            formData
          );
          return res.data;
        } catch (Ex) {
          showError("Failed to upload image");
          return undefined;
        }
      },
      deleteAttachment: async (
        deviceIdentifier: string,
        attachmentIdentifier: string
      ) => {
        try {
          let res = await apiClient.delete<any, AxiosResponse<DeviceInfo>>(
            `/api/devices/attachment/${deviceIdentifier}/${attachmentIdentifier}`
          );
          return res.data;
        } catch (Ex) {
          showError("Deleting attachment failed");
          throw Ex;
        }
      },
    },
    locationHook: {
      getLocations: async () => {
        try {
          let res = await apiClient.get<any, AxiosResponse<LocationModel[]>>(
            `/api/locations/`
          );
          return res.data;
        } catch (ex) {
          showError("Failed to get locations");
          return [];
        }
      },
    },
  };
};
