import type { AggregationTypes } from "../enums/aggregationTypes";

export interface AddOrUpdateSensor {
  identifier?: string;
  deviceIdentifier: string;
  sensorId?: number;
  name: string;
  scaleMin?: number;
  scaleMax?: number;
  active?: boolean;
  isVirtual?: boolean;
  aggregationType?: AggregationTypes;
}
