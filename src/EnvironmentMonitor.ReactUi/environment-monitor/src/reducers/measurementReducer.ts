import { createSlice } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";
import { RootState } from "../setup/appStore";

export interface MeasurementState {
  devices: Device[];
  sensors: Sensor[];
  autoScaleSensorIds: number[];
}

export interface DashboardAutoScale {
  deviceId: number;
  state: boolean;
}

const initialState: MeasurementState = {
  devices: [],
  sensors: [],
  autoScaleSensorIds: [],
};

export const measurementSlice = createSlice({
  name: "measurement",
  initialState,
  reducers: {
    setSensors: (state, action: PayloadAction<Sensor[]>) => {
      state.sensors = action.payload;
    },
    setDevices: (state, action: PayloadAction<Device[]>) => {
      state.devices = action.payload;
    },
    toggleAutoScale: (state, action: PayloadAction<DashboardAutoScale>) => {
      //
      if (action.payload.state) {
        if (
          !state.autoScaleSensorIds.some((s) => s === action.payload.deviceId)
        ) {
          state.autoScaleSensorIds.push(action.payload.deviceId);
        }
      } else {
        state.autoScaleSensorIds = state.autoScaleSensorIds.filter(
          (s) => s !== action.payload.deviceId
        );
      }
    },
  },
});

export const { setDevices, setSensors, toggleAutoScale } =
  measurementSlice.actions;

export const getDevices = (state: RootState): Device[] =>
  state.measurementInfo.devices;

export const getSensors = (state: RootState): Sensor[] =>
  state.measurementInfo.sensors;

export const getDashboardAutoScale = (state: RootState): number[] => {
  return state.measurementInfo.autoScaleSensorIds;
};

export default measurementSlice.reducer;
