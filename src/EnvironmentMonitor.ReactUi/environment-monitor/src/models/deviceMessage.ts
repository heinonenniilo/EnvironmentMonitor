export interface DeviceMessage {
  deviceId: number;
  identifier: string;
  timeStamp: Date;
  isDuplicate: boolean;
  uptime: number;
  id: number;
  messageCount: number;
  firstMessage: boolean;
}
