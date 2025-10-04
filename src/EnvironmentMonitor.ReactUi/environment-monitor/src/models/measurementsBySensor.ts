import type { Measurement } from "./measurement";
import type { Sensor } from "./sensor";

export interface MeasurementsViewModel {
  measurements: MeasurementsBySensor[];
  sensors?: Sensor[];
}

export interface MeasurementsBySensor {
  sensorIdentifier: string;
  measurements: Measurement[];
  minValues: { [key: number]: Measurement };
  maxValues: { [key: number]: Measurement };
  latestValues: { [key: number]: Measurement };
}

// const myDict: { [key: number]: Measurement } = {};

export interface MeasurementsModel {
  measurements: Measurement[];
  measurementsInfo: MeasurementsInfo;
}

export interface MeasurementsInfo {
  sensorIdentifier: string;
  minValues: { [key: number]: Measurement };
  maxValues: { [key: number]: Measurement };
  latestValues: { [key: number]: Measurement };
}

export interface MeasurementsByLocation {
  identifier: string;
  name: string;
  measurements: MeasurementsBySensor[];
  sensors: Sensor[];
}

export interface MeasurementsByLocationModel {
  measurements: MeasurementsByLocation[];
}
