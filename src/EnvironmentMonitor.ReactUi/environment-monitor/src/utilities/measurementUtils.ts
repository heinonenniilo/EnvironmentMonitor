import { MeasurementTypes } from "../enums/temperatureTypes";
import { Measurement } from "../models/measurement";
import { getFormattedDate } from "./datetimeUtils";

export const formatMeasurement = (
  measurement: Measurement,
  onlyValue?: boolean
) => {
  if (onlyValue) {
    return `${measurement.sensorValue.toFixed(2)} ${getMeasurementUnit(
      measurement.typeId
    )}`;
  }
  const formattedDate = getFormattedDate(measurement.timestamp);
  return `${measurement.sensorValue.toFixed(2)} ${getMeasurementUnit(
    measurement.typeId
  )} (${formattedDate})`;
};

export const getMeasurementUnit = (type: MeasurementTypes) => {
  switch (type) {
    case MeasurementTypes.Humidity:
      return "%";
    case MeasurementTypes.Temperature:
      return "°C";
    default:
      return "";
  }
};
