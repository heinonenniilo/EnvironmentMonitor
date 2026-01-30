export enum CommunicationChannels {
  IotHub = 0,
  RestApi = 1,
}

export const getCommunicationChannelDisplayName = (
  channelId: CommunicationChannels,
): string => {
  switch (channelId) {
    case CommunicationChannels.IotHub:
      return "IoT Hub";
    case CommunicationChannels.RestApi:
      return "Rest API";
    default:
      return "Unknown";
  }
};
