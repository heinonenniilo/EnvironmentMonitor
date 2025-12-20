import type { Device } from "./device";
import type { DeviceAttachment } from "./deviceAttachment";
import type { DeviceAttribute } from "./deviceAttribute";
import type { DeviceContact } from "./deviceContact";
import type { SensorInfo } from "./sensor";

export interface DeviceInfo {
  device: Device;
  onlineSince?: Date;
  rebootedOn?: Date;
  lastMessage?: Date;
  showWarning?: boolean;
  attachments: DeviceAttachment[];
  sensors: SensorInfo[];
  defaultImageGuid: string;
  deviceIdentifier: string;
  isVirtual: boolean;
  attributes: DeviceAttribute[];
  contacts: DeviceContact[];
}
