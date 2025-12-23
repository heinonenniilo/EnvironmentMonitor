export interface DeviceEmailTemplateDto {
  identifier: string;
  title?: string;
  message?: string;
  createdUtc: Date;
  created?: Date;
  updated?: Date;
  displayName: string;
}

export interface UpdateDeviceEmailTemplateDto {
  identifier: string;
  title?: string;
  message?: string;
}
