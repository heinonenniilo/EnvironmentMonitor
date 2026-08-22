export interface GetDeviceInfosModel {
  identifiers?: string[];
  locationIdentifiers?: string[];
  communicationChannelIds?: number[];
  onlyVisible: boolean;
  isVirtual?: boolean;
  getLatestMeasurementBySensor: boolean;
}
