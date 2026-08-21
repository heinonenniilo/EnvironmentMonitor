export interface GetDeviceInfosModel {
  identifiers?: string[];
  locationIdentifiers?: string[];
  communicationChannelIds?: number[];
  onlyVisible: boolean;
  getAttachments: boolean;
  getLocation: boolean;
  getAttributes: boolean;
  getContacts: boolean;
  isVirtual?: boolean;
  getLatestMeasurementBySensor: boolean;
}
