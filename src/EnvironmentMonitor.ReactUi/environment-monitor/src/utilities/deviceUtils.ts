import type { Device } from "../models/device";

export const getDeviceTitle = (device: Device | undefined): string => {
  if (!device) {
    return "";
  }
  return device.displayName ?? device.name;
};

export const getDeviceDefaultImageUrl = (identifier: string) => {
  return `/api/Devices/${identifier}/default-image`;
};

export const getDeviceAttachmentUrl = (
  deviceId: string,
  attachmentId: string
) => {
  return `/api/devices/${deviceId}/attachment/${attachmentId}`;
};
