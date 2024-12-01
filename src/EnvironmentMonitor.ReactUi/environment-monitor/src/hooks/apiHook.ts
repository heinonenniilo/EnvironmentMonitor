import axios, { AxiosResponse } from "axios";
import { User } from "../models/user";
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";
import qs from "qs";
import {
  MeasurementsModel,
  MeasurementsViewModel,
} from "../models/measurementsBySensor";

interface ApiHook {
  userHook: userHook;
  measureHook: measureHook;
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
    from: Date,
    to: Date
  ) => Promise<MeasurementsModel | undefined>;
  getMeasurementsBySensor: (
    sensorIds: number[],
    from: Date,
    to: Date | undefined,
    latestOnly?: boolean
  ) => Promise<MeasurementsViewModel | undefined>;
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
            "/authentication/info"
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
            "/authentication/login",
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
            "/authentication/logout"
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
            "/Measurements/devices"
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
            `/Measurements/sensors/`,
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
      getMeasurements: async (sensorIds: number[], from: Date, to: Date) => {
        try {
          let res = await apiClient.get<any, AxiosResponse<MeasurementsModel>>(
            "/Measurements",
            {
              params: {
                SensorIds: sensorIds,
                from: from,
                to: to,
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
        from: Date,
        to: Date | undefined,
        latestOnly?: boolean
      ) => {
        try {
          let res = await apiClient.get<
            any,
            AxiosResponse<MeasurementsViewModel>
          >("/Measurements/bysensor", {
            params: {
              SensorIds: sensorIds,
              from: from,
              to: to,
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
  };
};

// https://localhost:7135/Measurements/bysensor?SensorIds=1&SensorIds=2&From=2024-11-24
