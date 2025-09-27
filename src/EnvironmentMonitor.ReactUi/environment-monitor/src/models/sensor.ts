export interface Sensor {
  identifier: string;
  name: string;
  deviceIdentifier: string;
  sensorId: number;
  scaleMin?: number;
  scaleMax?: number;
}
