export interface AddOrUpdateSensor {
  identifier?: string;
  deviceIdentifier: string;
  sensorId?: number;
  name: string;
  scaleMin?: number;
  scaleMax?: number;
}
