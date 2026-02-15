export interface AddOrUpdatePublicSensorDto {
  identifier?: string;
  name: string;
  sensorIdentifier: string;
  typeId?: number;
  active?: boolean;
}

export interface GetPublicSensorsModel {
  identifiers?: string[];
  isActive?: boolean;
}

export interface ManagePublicSensorsRequest {
  addOrUpdate: AddOrUpdatePublicSensorDto[];
  remove: string[];
}
