import { Sensor } from "./sensor";

export interface Device {
  deviceIdentifier: string;
  id: number;
  name: string;
  visible: boolean;
  sensors: Sensor[];
}
