import type { CommunicationChannels } from "../enums/communicationChannels";

export interface DeviceMessage {
  deviceIdentifier: string;
  identifier: string;
  timeStamp: Date;
  isDuplicate: boolean;
  uptime: number;
  messageCount: number;
  firstMessage: boolean;
  sourceId: CommunicationChannels;
  //
  uniqueRowId?: string; // Calculated
}
