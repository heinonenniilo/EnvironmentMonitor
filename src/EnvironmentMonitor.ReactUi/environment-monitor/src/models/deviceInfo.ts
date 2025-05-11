import type { Device } from "./device";
import type { DeviceAttachment } from "./deviceAttachment";

export interface DeviceInfo {
  device: Device;
  onlineSince?: Date;
  rebootedOn?: Date;
  lastMessage?: Date;
  showWarning?: boolean;
  attachments: DeviceAttachment[];
  defaultImageGuid: string;
}
