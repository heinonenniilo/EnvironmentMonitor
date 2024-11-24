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
