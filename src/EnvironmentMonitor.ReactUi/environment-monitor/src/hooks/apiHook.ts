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
            "/api/Measurements/devices"
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
            `/api/Measurements/sensors/`,
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
            "/api/Measurements",
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
          >("/api/Measurements/bysensor", {
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
