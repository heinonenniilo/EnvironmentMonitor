/*
import { produce } from "immer";
import {
  MeasurementActionTypes,
  MeasurementActions,
} from "../actions/measurementActions";
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";
import { AppState } from "../setup/appRootReducer";

export interface MeasurementState {
  devices: Device[];
  sensors: Sensor[];
}

const defaultState: MeasurementState = {
  devices: [],
  sensors: [],
};

export function measurementReducer(
  state: MeasurementState = defaultState,
  action: MeasurementActions
): MeasurementState {
  switch (action.type) {
    case MeasurementActionTypes.SetDevices:
      state = produce(state, (draft) => {
        draft.devices = action.devices ?? [];
      });
      break;

    case MeasurementActionTypes.SetSensors:
      state = produce(state, (draft) => {
        draft.sensors = action.sensors;
      });
      break;
  }
  return state;
}

export const getDevices = (state: AppState): Device[] =>
  state.measurementInfo.devices;

export const getSensors = (state: AppState): Sensor[] =>
  state.measurementInfo.sensors;
*/
import { createSlice } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";
import { RootState } from "../setup/appStore";

export interface MeasurementState {
  devices: Device[];
  sensors: Sensor[];
}

const initialState: MeasurementState = {
  devices: [],
  sensors: [],
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
  },
});

export const { setDevices, setSensors } = measurementSlice.actions;

export const getDevices = (state: RootState): Device[] =>
  state.measurementInfo.devices;

export const getSensors = (state: RootState): Sensor[] =>
  state.measurementInfo.sensors;

export default measurementSlice.reducer;
