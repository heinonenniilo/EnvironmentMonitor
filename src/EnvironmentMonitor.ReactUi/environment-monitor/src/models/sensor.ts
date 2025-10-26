export interface Sensor {
  identifier: string;
  name: string;
  deviceIdentifier: string;

  scaleMin?: number;
  scaleMax?: number;
}

export interface SensorInfo extends Sensor {
  sensorId: number;
  sensors: VirtualSensor[];
  isVirtual: boolean;
}

export interface VirtualSensor {
  sensor: Sensor;
  identifier: string;
  typeId: number;
}
