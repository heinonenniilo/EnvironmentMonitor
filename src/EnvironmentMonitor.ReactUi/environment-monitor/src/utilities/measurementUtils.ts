import { MeasurementTypes } from "../enums/temperatureTypes";
import { Measurement } from "../models/measurement";
import { getFormattedDate } from "./datetimeUtils";

export const formatMeasurement = (measurement: Measurement) => {
  const formattedDate = getFormattedDate(measurement.timestamp);
  return `${measurement.sensorValue.toFixed(2)} ${getUnit(
    measurement.typeId
  )} (${formattedDate})`;
};

const getUnit = (type: MeasurementTypes) => {
  switch (type) {
    case MeasurementTypes.Humidity:
      return "%";
    case MeasurementTypes.Temperature:
      return "°C";
    default:
      return "";
  }
};
