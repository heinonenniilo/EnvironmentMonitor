export enum CommunicationChannels {
  IotHub = 0,
  RestApi = 1,
  Ilmatieteenlaitos = 2,
}

export const getCommunicationChannelDisplayName = (
  channelId: CommunicationChannels,
): string => {
  switch (channelId) {
    case CommunicationChannels.IotHub:
      return "IoT Hub";
    case CommunicationChannels.RestApi:
      return "Rest API";
    case CommunicationChannels.Ilmatieteenlaitos:
      return "Ilmatieteenlaitos API";
    default:
      return "Unknown";
  }
};
