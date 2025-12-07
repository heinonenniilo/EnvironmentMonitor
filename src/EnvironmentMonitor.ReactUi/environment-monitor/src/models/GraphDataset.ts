import type { MeasurementTypes } from "../enums/measurementTypes";

export interface GraphDataset {
  label: string;
  yAxisID: string;
  data: {
    x: Date;
    y: number;
  }[];
  id: number;
  measurementType: MeasurementTypes;
  borderColor?: string;
  backgroundColor?: string;
  stepped?: boolean;
  sensorIdentifier: string;
}
