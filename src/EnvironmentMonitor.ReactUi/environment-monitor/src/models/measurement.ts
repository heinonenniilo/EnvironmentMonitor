export interface Measurement {
  sensorIdentifier: string;
  sensorValue: number;
  typeId: number;
  timestamp: Date;
  id?: number;
}
