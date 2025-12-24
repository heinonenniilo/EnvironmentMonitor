export interface CopyQueuedCommand {
  deviceIdentifier: string;
  messageId: string;
  scheduledTime?: Date;
}
