export interface AddOrUpdateDeviceDto {
  identifier?: string;
  name: string;
  deviceIdentifier: string;
  visible: boolean;
  isVirtual: boolean;
  communicationChannelId?: number;
  locationIdentifier?: string;
}
