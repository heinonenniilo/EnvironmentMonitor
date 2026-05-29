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
  created: Date;
  updated?: Date;
  lastMeasurement?: Date;
  showWarning: boolean;
}

export interface VirtualSensor {
  sensor: Sensor;
  identifier: string;
  typeId: number;
}
