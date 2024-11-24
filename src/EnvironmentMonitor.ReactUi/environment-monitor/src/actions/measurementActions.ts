import { Action } from "redux";
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";
export enum MeasurementActionTypes {
  SetDevices = "Measurement/SetDevices",
  SetSensors = "Measurement/SetSensors",
}

export interface SetDevices extends Action {
  type: MeasurementActionTypes.SetDevices;
  devices: Device[];
}

export interface SetSensors extends Action {
  type: MeasurementActionTypes.SetSensors;
  sensors: Sensor[];
}

export const measurementActions = {
  setDevices: (devices: Device[]) => ({
    type: MeasurementActionTypes.SetDevices,
    devices,
  }),
  setSensors: (sensors: Sensor[]) => ({
    type: MeasurementActionTypes.SetSensors,
    sensors,
  }),
};

export type MeasurementActions = SetDevices | SetSensors;
