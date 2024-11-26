import moment from "moment";

export const datetimeFormat = "DD.MM.YYYY HH:mm";

export const getFormattedDate = (date: Date): string => {
  const formattedDate = moment(date).format(datetimeFormat);
  return formattedDate;
};
