import type { Sensor } from "./sensor";

export interface Device {
  identifier: string;
  name: string;
  visible: boolean;
  hasMotionSensor: boolean;
  sensors: Sensor[];
  locationIdentifier?: string;
  displayName?: string;
}
