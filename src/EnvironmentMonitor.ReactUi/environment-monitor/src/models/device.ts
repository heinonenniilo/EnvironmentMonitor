import type { Sensor } from "./sensor";

export interface Device {
  identifier: string;
  id: number;
  name: string;
  visible: boolean;
  hasMotionSensor: boolean;
  sensors: Sensor[];
  locationId?: number;
  displayName?: string;
}
