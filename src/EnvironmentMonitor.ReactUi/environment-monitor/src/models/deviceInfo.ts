import { Device } from "./device";
import { DeviceAttachment } from "./deviceAttachment";

export interface DeviceInfo {
  device: Device;
  onlineSince?: Date;
  rebootedOn?: Date;
  lastMessage?: Date;
  showWarning?: boolean;
  attachments: DeviceAttachment[];
}
