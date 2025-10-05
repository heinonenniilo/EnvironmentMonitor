export interface GetMeasurementsModel {
  sensorIdentifiers: string[];
  deviceMessageIdentifiers?: string[];
  deviceIdentifiers?: string[];
  latestOnly?: boolean;
  from?: moment.Moment;
  to?: moment.Moment;
}
