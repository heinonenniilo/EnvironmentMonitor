import { MeasurementTypes } from "../enums/temperatureTypes";
import { Measurement } from "./measurement";

export interface MeasurementsViewModel {
  measurements: MeasurementsBySensor[];
}

export interface MeasurementsBySensor {
  sensorId: number;
  measurements: Measurement[];
  MinValues: Record<MeasurementTypes, Measurement>;
  MaxValues: Record<MeasurementTypes, Measurement>;
}
