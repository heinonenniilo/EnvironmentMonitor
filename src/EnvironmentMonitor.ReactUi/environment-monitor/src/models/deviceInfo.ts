import type { Device } from "./device";
import type { DeviceAttachment } from "./deviceAttachment";
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
}
