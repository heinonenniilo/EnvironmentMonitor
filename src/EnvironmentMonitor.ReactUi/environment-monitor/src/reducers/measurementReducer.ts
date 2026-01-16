import { createSelector, createSlice } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";
import type { Device } from "../models/device";
import type { Sensor } from "../models/sensor";
import type { LocationModel } from "../models/location";
import type { RootState } from "../setup/appStore";
import type { DeviceInfo } from "../models/deviceInfo";
import { MeasurementTypes } from "../enums/measurementTypes";

// Get all available measurement types (excluding Undefined and Online)
const getDefaultMeasurementTypes = (): number[] => {
  return Object.keys(MeasurementTypes)
    .filter(
      (key) =>
        !isNaN(Number(MeasurementTypes[key as keyof typeof MeasurementTypes]))
    )
    .map((key) =>
      Number(MeasurementTypes[key as keyof typeof MeasurementTypes])
    )
    .filter(
      (value) =>
        value !== MeasurementTypes.Undefined &&
        value !== MeasurementTypes.Online
    );
};

export interface MeasurementState {
  devices: Device[];
  sensors: Sensor[];
  deviceInfos: DeviceInfo[];

  locations: LocationModel[];
  autoScaleSensorIds: string[];
  timeRange: number;
  selectedMeasurementTypes: number[];
}

export interface DashboardAutoScale {
  deviceIdentifier: string;
  state: boolean;
}

const initialState: MeasurementState = {
  devices: [],
  deviceInfos: [],
  sensors: [],
  autoScaleSensorIds: [],
  timeRange: 24,
  locations: [],
  selectedMeasurementTypes: getDefaultMeasurementTypes(),
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
    setDeviceInfos: (state, action: PayloadAction<DeviceInfo[]>) => {
      state.deviceInfos = action.payload;
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
    setSelectedMeasurementTypes: (state, action: PayloadAction<number[]>) => {
      state.selectedMeasurementTypes = action.payload;
    },
  },
});

export const {
  setDevices,
  setDeviceInfos,
  setSensors,
  toggleAutoScale,
  setDashboardTimeRange,
  setLocations,
  setSelectedMeasurementTypes,
} = measurementSlice.actions;

export const getDevices = (state: RootState): Device[] =>
  state.measurementInfo.devices;

export const getDeviceInfos = (state: RootState): DeviceInfo[] =>
  state.measurementInfo.deviceInfos;

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

export const getSelectedMeasurementTypes = (state: RootState): number[] => {
  return state.measurementInfo.selectedMeasurementTypes;
};

export default measurementSlice.reducer;
