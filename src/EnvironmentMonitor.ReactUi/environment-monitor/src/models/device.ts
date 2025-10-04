export interface Device {
  identifier: string;
  name: string;
  visible: boolean;
  hasMotionSensor: boolean;
  locationIdentifier?: string;
  displayName?: string;
}
