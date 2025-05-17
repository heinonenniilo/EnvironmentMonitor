import type { Device } from "../models/device";

export const getDeviceTitle = (device: Device | undefined): string => {
  if (!device) {
    return "";
  }
  return device.displayName ?? device.name;
};
