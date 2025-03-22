import { MeasurementTypes } from "../enums/measurementTypes";
import { Measurement } from "../models/measurement";
import { getFormattedDate } from "./datetimeUtils";

export const formatMeasurement = (
  measurement: Measurement,
  onlyValue?: boolean
) => {
  if (onlyValue) {
    return `${formatMeasurementValue(measurement)} ${getMeasurementUnit(
      measurement.typeId
    )}`;
  }
  const formattedDate = getFormattedDate(measurement.timestamp);
  return `${formatMeasurementValue(measurement)} ${getMeasurementUnit(
    measurement.typeId
  )} (${formattedDate})`;
};

export const getMeasurementUnit = (type: MeasurementTypes) => {
  switch (type) {
    case MeasurementTypes.Humidity:
      return "%";
    case MeasurementTypes.Temperature:
      return "°C";
    case MeasurementTypes.Light:
      return "lx";
    case MeasurementTypes.Motion:
      return "";
    default:
      return "";
  }
};

const formatMeasurementValue = (measurement: Measurement) => {
  if (measurement.typeId === MeasurementTypes.Motion) {
    return measurement.sensorValue > 0 ? "ON" : "OFF";
  } else {
    return measurement.sensorValue.toFixed(2);
  }
};

export const getDatasetLabel = (
  sensorName: string,
  measurementType?: MeasurementTypes
) => {
  const unit = measurementType ? getMeasurementUnit(measurementType) : "";
  if (unit === "") {
    return sensorName;
  }
  return `${sensorName} (${unit})`;
};
