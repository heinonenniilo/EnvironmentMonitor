import { Device } from "./device";

export interface DeviceInfo {
  device: Device;
  onlineSince?: Date;
  rebootedOn?: Date;
  lastMessage?: Date;
  showWarning?: boolean;
}
