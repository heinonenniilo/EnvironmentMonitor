import type { PaginationParams } from "./paginationParams";
export interface GetDeviceMessagesModel extends PaginationParams {
  deviceIdentifiers?: string[];
  locationIdentifiers?: string[];
  sourceIds?: number[];
  isDuplicate?: boolean;
  isFirstMessage?: boolean;
  from: moment.Moment;
  to?: moment.Moment;
}
