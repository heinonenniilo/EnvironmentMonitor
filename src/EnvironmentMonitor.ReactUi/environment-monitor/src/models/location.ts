import type { Entity } from "./entity";
import type { Sensor } from "./sensor";

export interface LocationModel extends Entity {
  locationSensors: Sensor[];
  visible: boolean;
}
