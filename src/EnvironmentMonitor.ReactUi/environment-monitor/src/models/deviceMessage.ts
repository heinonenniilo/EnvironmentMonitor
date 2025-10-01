export interface DeviceMessage {
  deviceIdentifier: string;
  identifier: string;
  timeStamp: Date;
  isDuplicate: boolean;
  uptime: number;
  id: number;
  messageCount: number;
  firstMessage: boolean;
}
