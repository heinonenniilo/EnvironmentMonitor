export interface GetMeasurementsModel {
  sensorIds: number[];
  deviceMessageIds?: number[];
  latestOnly?: boolean;
  from?: moment.Moment;
  to?: moment.Moment;
}
