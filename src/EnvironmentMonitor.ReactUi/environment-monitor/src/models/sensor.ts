export interface Sensor {
  id: number;
  name: string;
  deviceId: number;
  sensorId: number;
  scaleMin?: number;
  scaleMax?: number;
}
