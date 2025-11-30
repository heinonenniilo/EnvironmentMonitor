export interface Sensor {
  identifier: string;
  name: string;
  parentIdentifier: string;

  scaleMin?: number;
  scaleMax?: number;
  measurementType?: number;
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
