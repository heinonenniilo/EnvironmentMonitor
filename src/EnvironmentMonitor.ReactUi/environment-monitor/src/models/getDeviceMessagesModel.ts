import type { PaginationParams } from "./paginationParams";
export interface GetDeviceMessagesModel extends PaginationParams {
  deviceIds?: number[];
  locationIds?: number[];
  isDuplicate?: boolean;
  isFirstMessage?: boolean;
  from: moment.Moment;
  to?: moment.Moment;
}
