import type { Entity } from "../models/entity";

export const getEntityTitle = (entity: Entity | undefined): string => {
  if (!entity) {
    return "";
  }
  return entity.displayName ?? entity.name;
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
