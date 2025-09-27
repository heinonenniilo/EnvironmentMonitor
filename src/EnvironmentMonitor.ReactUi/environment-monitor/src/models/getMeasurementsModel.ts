export interface GetMeasurementsModel {
  sensorIdentifiers: string[];
  deviceMessageIds?: number[];
  latestOnly?: boolean;
  from?: moment.Moment;
  to?: moment.Moment;
}
