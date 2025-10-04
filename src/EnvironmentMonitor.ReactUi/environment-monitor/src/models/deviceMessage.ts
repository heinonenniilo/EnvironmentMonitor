export interface DeviceMessage {
  deviceIdentifier: string;
  identifier: string;
  timeStamp: Date;
  isDuplicate: boolean;
  uptime: number;
  messageCount: number;
  firstMessage: boolean;
  //
  uniqueRowId?: string; // Calculated
}
