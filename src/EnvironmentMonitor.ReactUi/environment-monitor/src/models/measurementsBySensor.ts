import type { Measurement } from "./measurement";
import type { Sensor } from "./sensor";

export interface MeasurementsViewModel {
  measurements: MeasurementsBySensor[];
}

export interface MeasurementsBySensor {
  sensorId: number;
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
  sensorId: number;
  minValues: { [key: number]: Measurement };
  maxValues: { [key: number]: Measurement };
  latestValues: { [key: number]: Measurement };
}

export interface MeasurementsByLocation {
  id: number;
  name: string;
  measurements: MeasurementsBySensor[];
  sensors: Sensor[];
}

export interface MeasurementsByLocationModel {
  measurements: MeasurementsByLocation[];
}
