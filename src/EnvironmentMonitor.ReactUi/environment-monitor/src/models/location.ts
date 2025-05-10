import type { Sensor } from "./sensor";

export interface LocationModel {
  name: string;
  locationSensors: Sensor[];
  id: number;
}
