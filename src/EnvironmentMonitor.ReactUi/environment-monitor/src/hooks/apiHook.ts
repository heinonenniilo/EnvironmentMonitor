import axios, { AxiosResponse } from "axios";
import { User } from "../models/user";
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";
import qs from "qs";
import {
  MeasurementsModel,
  MeasurementsViewModel,
} from "../models/measurementsBySensor";
import moment from "moment";
import { DeviceInfo } from "../models/deviceInfo";

interface ApiHook {
  userHook: userHook;
  measureHook: measureHook;
  deviceHook: deviceHook;
}

interface userHook {
  getUserInfo: () => Promise<User | undefined>;
  logIn: (userId: string, password: string) => Promise<boolean>;
  logOut: () => Promise<boolean>;
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
    delay: number
  ) => Promise<boolean>;
}

const apiClient = axios.create({
  baseURL: undefined,
  //process.env.NODE_ENV === "production"
  //  ? undefined
  //  : "https://localhost:7135",
  withCredentials: true,
  paramsSerializer: (params) => {
    return qs.stringify(params, { arrayFormat: "repeat" }); // or "comma" if backend expects "1,2,3"
  },
});

export const useApiHook = (): ApiHook => {
  return {
    userHook: {
      getUserInfo: async () => {
        try {
          const response = await apiClient.get<any, AxiosResponse<User>>(
            "/api/authentication/info"
          );
          return response.data;
        } catch (ex: any) {
          console.error(ex);
          return undefined;
        }
      },
      logIn: async (userId, password) => {
        try {
          const response = await apiClient.post<any, AxiosResponse<boolean>>(
            "/api/authentication/login",
            {
              email: userId,
              password: password,
            }
          );
          return response.data;
        } catch (ex: any) {
          console.error(ex);
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
          console.log(ex);
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
        let res = await apiClient.post("/api/devices/motion-control-status", {
          deviceIdentifier: identifier,
          mode: state,
        });
        if (res.status === 200) {
          return true;
        } else {
          return false;
        }
      },
      setMotionControlDelay: async (identifier: string, delay: number) => {
        let res = await apiClient.post("/api/devices/motion-control-delay", {
          deviceIdentifier: identifier,
          DelayMs: delay,
        });
        if (res.status === 200) {
          return true;
        } else {
          return false;
        }
      },
    },
  };
};
