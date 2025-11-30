export interface GetMeasurementsModel {
  sensorIdentifiers: string[];
  deviceMessageIdentifiers?: string[];
  deviceIdentifiers?: string[];
  locationIdentifiers?: string[];
  latestOnly?: boolean;
  from?: moment.Moment;
  to?: moment.Moment;
}
