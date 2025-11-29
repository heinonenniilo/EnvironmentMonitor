import type { Entity } from "../models/entity";

export const getDeviceTitle = (device: Entity | undefined): string => {
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
