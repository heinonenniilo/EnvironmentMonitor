import type { PaginationParams } from "./paginationParams";
export interface GetDeviceMessagesModel extends PaginationParams {
  deviceIds?: number[];
  isDuplicate?: boolean;
  from: moment.Moment;
  to?: moment.Moment;
}
