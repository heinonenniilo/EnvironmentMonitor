import moment from "moment";

export const datetimeFormat = "DD.MM. HH:mm";
export const dateTimeFormatLong = "DD.MM.YYYY HH:mm";

export const getFormattedDate = (
  date: Date,
  includeYear?: boolean,
  includeSeconds?: boolean
): string => {
  let format = includeYear ? dateTimeFormatLong : datetimeFormat;

  if (includeSeconds) {
    format += ":ss";
  }
  return moment(date).format(format);
};

export const dateTimeSort = (
  a: Date | undefined,
  b: Date | undefined
): number => {
  if (!a && !b) {
    return 0;
  }
  if (a === undefined) {
    return 1;
  }
  if (b === undefined) {
    return -1;
  }
  return new Date(b).getTime() - new Date(a).getTime();
};
