import { createSelector, createSlice } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";
import type { Device } from "../models/device";
import type { Sensor } from "../models/sensor";
import type { LocationModel } from "../models/location";
import type { RootState } from "../setup/appStore";

export interface MeasurementState {
  devices: Device[];
  sensors: Sensor[];

  locations: LocationModel[];
  autoScaleSensorIds: string[];
  timeRange: number;
}

export interface DashboardAutoScale {
  deviceIdentifier: string;
  state: boolean;
}

const initialState: MeasurementState = {
  devices: [],
  sensors: [],
  autoScaleSensorIds: [],
  timeRange: 24,
  locations: [],
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
      if (action.payload.state) {
        if (
          !state.autoScaleSensorIds.some(
            (s) => s === action.payload.deviceIdentifier
          )
        ) {
          state.autoScaleSensorIds.push(action.payload.deviceIdentifier);
        }
      } else {
        state.autoScaleSensorIds = state.autoScaleSensorIds.filter(
          (s) => s !== action.payload.deviceIdentifier
        );
      }
    },
    setDashboardTimeRange: (state, action: PayloadAction<number>) => {
      state.timeRange = action.payload;
    },
    setLocations: (state, action: PayloadAction<LocationModel[]>) => {
      state.locations = action.payload;
    },
  },
});

export const {
  setDevices,
  setSensors,
  toggleAutoScale,
  setDashboardTimeRange,
  setLocations,
} = measurementSlice.actions;

export const getDevices = (state: RootState): Device[] =>
  state.measurementInfo.devices;

export const getSensors = (state: RootState): Sensor[] =>
  state.measurementInfo.sensors;

export const getDashboardAutoScale = (state: RootState): string[] => {
  return state.measurementInfo.autoScaleSensorIds;
};

export const getLocations = (state: RootState): LocationModel[] =>
  state.measurementInfo.locations;

export const getDeviceAutoScale = (deviceIdentifier: string) =>
  createSelector(
    [(state: RootState) => state.measurementInfo.autoScaleSensorIds],
    (autoScaleSensorIds) => autoScaleSensorIds.includes(deviceIdentifier)
  );

export const getDashboardTimeRange = (state: RootState): number => {
  return state.measurementInfo.timeRange;
};

export default measurementSlice.reducer;
