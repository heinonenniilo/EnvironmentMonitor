export interface AddOrUpdatePublicSensorDto {
  identifier?: string;
  name: string;
  sensorIdentifier: string;
  typeId?: number;
}

export interface ManagePublicSensorsRequest {
  addOrUpdate: AddOrUpdatePublicSensorDto[];
  remove: string[];
}
