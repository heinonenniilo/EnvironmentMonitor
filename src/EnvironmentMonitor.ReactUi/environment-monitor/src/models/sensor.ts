export interface Sensor {
  identifier: string;
  name: string;
  deviceIdentifier: string;

  scaleMin?: number;
  scaleMax?: number;
}

export interface SensorInfo extends Sensor {
  sensorId: number;
}
