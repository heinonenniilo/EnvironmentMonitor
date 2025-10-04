export interface GetMeasurementsModel {
  sensorIdentifiers: string[];
  deviceMessageIdentifiers?: string[];
  latestOnly?: boolean;
  from?: moment.Moment;
  to?: moment.Moment;
}
