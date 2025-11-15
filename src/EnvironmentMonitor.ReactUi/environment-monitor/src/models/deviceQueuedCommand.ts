export interface DeviceQueuedCommandDto {
  messageId: string;
  deviceIdentifier: string;
  scheduled: Date;
  executedAt?: Date;
  type: number;
  message: string;
  created: Date;
}
