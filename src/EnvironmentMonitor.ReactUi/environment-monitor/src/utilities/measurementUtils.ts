import { MeasurementTypes } from "../enums/measurementTypes";
import type { Measurement } from "../models/measurement";
import { getFormattedDate } from "./datetimeUtils";

export const formatMeasurement = (
  measurement: Measurement,
  onlyValue?: boolean,
  includeSeconds?: boolean,
) => {
  const formattedValue =
    measurement.typeId === MeasurementTypes.Motion
      ? formatMeasurementValue(measurement)
      : `${formatMeasurementValue(measurement)} ${getMeasurementUnit(
          measurement.typeId,
        )}`;
  if (onlyValue) {
    return `${formattedValue}`;
  }
  const formattedDate = getFormattedDate(
    measurement.timestamp,
    false,
    includeSeconds,
  );

  return `${formattedValue} (${formattedDate})`;
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
      return "motion";
    default:
      return "";
  }
};

const formatMeasurementValue = (measurement: Measurement) => {
  if (
    measurement.typeId === MeasurementTypes.Motion ||
    measurement.typeId === MeasurementTypes.Online
  ) {
    return measurement.sensorValue > 0 ? "ON" : "OFF";
  } else {
    return measurement.sensorValue.toFixed(2);
  }
};

export const getDatasetLabel = (
  sensorName: string,
  measurementType?: MeasurementTypes,
) => {
  const unit = measurementType ? getMeasurementUnit(measurementType) : "";
  if (unit === "") {
    return sensorName;
  }
  return `${sensorName} (${unit})`;
};

export const getMeasurementTypeDisplayName = (type: MeasurementTypes) => {
  switch (type) {
    case MeasurementTypes.Temperature:
      return "Temperature";
    case MeasurementTypes.Humidity:
      return "Humidity";
    case MeasurementTypes.Light:
      return "Light";
    case MeasurementTypes.Motion:
      return "Motion";
    case MeasurementTypes.Online:
      return "Online Status";
    case MeasurementTypes.Undefined:
      return "Undefined";
    default:
      return "Unknown";
  }
};

export const getAvailableMeasurementTypes = (): number[] => {
  return Object.keys(MeasurementTypes)
    .filter(
      (key) =>
        !isNaN(Number(MeasurementTypes[key as keyof typeof MeasurementTypes])),
    )
    .map((key) =>
      Number(MeasurementTypes[key as keyof typeof MeasurementTypes]),
    )
    .filter(
      (value) =>
        value !== MeasurementTypes.Undefined &&
        value !== MeasurementTypes.Online,
    );
};
