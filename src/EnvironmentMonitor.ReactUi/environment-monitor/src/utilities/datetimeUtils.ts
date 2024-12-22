import moment from "moment";

export const datetimeFormat = "DD.MM. HH:mm";
export const dateTimeFormatLong = "DD.MM.YYYY HH:mm";

export const getFormattedDate = (date: Date, includeYear?: boolean): string => {
  const formattedDate = moment(date).format(
    includeYear ? dateTimeFormatLong : datetimeFormat
  );
  return formattedDate;
};
