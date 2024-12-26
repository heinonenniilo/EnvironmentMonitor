import moment from "moment";

export const datetimeFormat = "DD.MM. HH:mm";
export const dateTimeFormatLong = "DD.MM.YYYY HH:mm";

export const getFormattedDate = (date: Date, includeYear?: boolean): string => {
  const formattedDate = moment(date).format(
    includeYear ? dateTimeFormatLong : datetimeFormat
  );
  return formattedDate;
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
