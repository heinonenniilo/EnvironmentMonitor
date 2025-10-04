import type { Sensor } from "./sensor";

export interface LocationModel {
  name: string;
  locationSensors: Sensor[];
  identifier: string;
  visible: boolean;
}
