import { Sensor } from "./sensor";

export interface Device {
  deviceIdentifier: string;
  id: number;
  name: string;
  visible: boolean;
  hasMotionSensor: boolean;
  hasDefaultImage: boolean;
  sensors: Sensor[];
}
