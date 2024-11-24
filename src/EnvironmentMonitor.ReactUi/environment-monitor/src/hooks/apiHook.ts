import axios, { AxiosResponse } from "axios";
import { User } from "../models/user";
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";
import { Measurement } from "../models/measurement";

interface ApiHook {
  //getUserInfo: () => Promise<User | undefined>;
  //logIn: (userId: string, password: string) => Promise<boolean>;
  // logOut: () => Promise<boolean>;
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
  getSensors: (deviceId: string) => Promise<Sensor[]>;
  getMeasurements: (
    sensorId: number,
    from: Date,
    to: Date
  ) => Promise<Measurement[]>;
}

const apiClient = axios.create({
  baseURL: undefined,
  //process.env.NODE_ENV === "production"
  //  ? undefined
  //  : "https://localhost:7135",
  withCredentials: true,
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
      getSensors: async (deviceId: string) => {
        try {
          let res = await apiClient.get<any, AxiosResponse<Sensor[]>>(
            `/Measurements/sensors/${deviceId}`
          );
          return res.data;
        } catch (ex: any) {
          console.error(ex);
          return [];
        }
      },
      getMeasurements: async (sensorId: number, from: Date, to: Date) => {
        try {
          let res = await apiClient.get<any, AxiosResponse<Measurement[]>>(
            "/Measurements",
            {
              params: {
                sensorId: sensorId,
                from: from,
                to: to,
              },
            }
          );
          return res.data;
        } catch (ex: any) {
          console.error(ex);
          return [];
        }
      },
    },
  };
};
