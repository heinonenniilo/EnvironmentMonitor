import type { Entity } from "./entity";

export interface Device extends Entity {
  visible: boolean;
  hasMotionSensor: boolean;
  locationIdentifier?: string;
  displayName?: string;
  autoScaleByDefault: boolean;
}
