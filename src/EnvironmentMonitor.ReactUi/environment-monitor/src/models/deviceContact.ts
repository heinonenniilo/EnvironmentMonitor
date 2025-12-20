export interface DeviceContact {
  identifier: string;
  email: string;
  created: Date;
  createdUtc: Date;
  updated?: Date;
  updatedUtc?: Date;
}
