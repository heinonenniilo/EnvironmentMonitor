import type { MeasurementSourceTypes } from "../enums/measurementSourceTypes";

export interface DeviceMessage {
  deviceIdentifier: string;
  identifier: string;
  timeStamp: Date;
  isDuplicate: boolean;
  uptime: number;
  messageCount: number;
  firstMessage: boolean;
  sourceId: MeasurementSourceTypes;
  //
  uniqueRowId?: string; // Calculated
}
