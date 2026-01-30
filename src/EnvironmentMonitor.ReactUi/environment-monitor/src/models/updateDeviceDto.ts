import type { Device } from "./device";

export interface UpdateDeviceDto {
  device: Device;
  communicationChannelId?: number;
}
