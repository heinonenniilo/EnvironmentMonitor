export enum MeasurementSourceTypes {
  IotHub = 0,
  Rest = 1,
}

export const getMeasurementSourceDisplayName = (
  sourceId: MeasurementSourceTypes,
): string => {
  switch (sourceId) {
    case MeasurementSourceTypes.IotHub:
      return "IoT Hub";
    case MeasurementSourceTypes.Rest:
      return "Rest interface";
    default:
      return "Unknown";
  }
};
