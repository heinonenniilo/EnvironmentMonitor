export enum MeasurementSourceTypes {
  IotHub = 0,
  Rest = 1,
  Ilmatieteenlaitos = 2,
}

export const getMeasurementSourceDisplayName = (
  sourceId: MeasurementSourceTypes,
): string => {
  switch (sourceId) {
    case MeasurementSourceTypes.IotHub:
      return "IoT Hub";
    case MeasurementSourceTypes.Rest:
      return "Rest interface";
    case MeasurementSourceTypes.Ilmatieteenlaitos:
      return "Ilmatieteen laitos (Open data)";
    default:
      return "Unknown";
  }
};
