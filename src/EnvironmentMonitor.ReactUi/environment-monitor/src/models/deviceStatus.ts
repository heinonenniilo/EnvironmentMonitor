export interface DeviceStatus {
  status: boolean;
  deviceId: number;
  timestamp: Date;
}

export interface DeviceStatusModel {
  deviceStatuses: DeviceStatus[];
}

export interface GetDeviceStatusModel {
  deviceIds: number[];
  from: Date;
  to?: Date;
}
