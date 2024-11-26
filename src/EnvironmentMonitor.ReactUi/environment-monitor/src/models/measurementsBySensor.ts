import { MeasurementTypes } from "../enums/temperatureTypes";
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
