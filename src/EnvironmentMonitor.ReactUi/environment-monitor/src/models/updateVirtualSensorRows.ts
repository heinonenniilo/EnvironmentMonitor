export interface AddVirtualSensorRowDto {
  valueSensorIdentifier: string;
  typeId?: number;
}

export interface UpdateVirtualSensorRowsDto {
  deviceIdentifier: string;
  sensorIdentifier: string;
  rowsToAdd: AddVirtualSensorRowDto[];
  rowsToDelete: string[];
}
