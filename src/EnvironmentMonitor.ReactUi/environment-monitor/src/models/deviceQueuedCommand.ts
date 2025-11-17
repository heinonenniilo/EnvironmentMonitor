export interface DeviceQueuedCommandDto {
  messageId: string;
  deviceIdentifier: string;
  scheduled: Date;
  executedAt?: Date;
  type: string;
  message: string;
  created: Date;
  isRemoved: boolean;
}
