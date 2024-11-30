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

/*
    public class MeasurementsInfoDto
    {
        public int SensorId { get; set; }
        public Dictionary<int, MeasurementDto> MinValues { get; set; } = [];
        public Dictionary<int, MeasurementDto> MaxValues { get; set; } = [];
        public Dictionary<int, MeasurementDto> LatestValues { get; set; } = [];
    }

*/
