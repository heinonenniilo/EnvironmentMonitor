export interface Sensor {
  identifier: string;
  name: string;
  displayName?: string;
  parentIdentifier: string;

  scaleMin?: number;
  scaleMax?: number;
  measurementType?: number;
  aggregationType?: number;
  active?: boolean;
  longitude?: number;
  latitude?: number;
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
