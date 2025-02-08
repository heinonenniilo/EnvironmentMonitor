import { MeasurementTypes } from "../enums/measurementTypes";
import { Measurement } from "./measurement";

export interface MeasurementsViewModel {
  measurements: MeasurementsBySensor[];
}

export interface MeasurementsBySensor {
  sensorId: number;
  measurements: Measurement[];
  minValues: Record<MeasurementTypes, Measurement>;
  maxValues: Record<MeasurementTypes, Measurement>;
  latestValues: Record<MeasurementTypes, Measurement>;
}

export interface MeasurementsModel {
  measurements: Measurement[];
  measurementsInfo: MeasurementsInfo;
}

export interface MeasurementsInfo {
  sensorId: number;
  minValues: Record<MeasurementTypes, Measurement>;
  maxValues: Record<MeasurementTypes, Measurement>;
  latestValues: Record<MeasurementTypes, Measurement>;
}
