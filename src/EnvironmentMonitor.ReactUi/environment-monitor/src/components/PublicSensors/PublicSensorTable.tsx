import {
  Box,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { Edit, Delete } from "@mui/icons-material";
import type { Sensor } from "../../models/sensor";
import { getMeasurementUnit } from "../../utilities/measurementUtils";
import type { MeasurementTypes } from "../../enums/measurementTypes";

export interface PublicSensorTableProps {
  sensors: Sensor[];
  allSensors: Sensor[];
  onEdit?: (sensor: Sensor) => void;
  onDelete?: (sensor: Sensor) => void;
}

export const PublicSensorTable: React.FC<PublicSensorTableProps> = ({
  sensors,
  allSensors,
  onEdit,
  onDelete,
}) => {
  return (
    <Box marginTop={1}>
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Source Sensor</TableCell>
              <TableCell>Type</TableCell>
              {(onEdit || onDelete) && (
                <TableCell align="right">Actions</TableCell>
              )}
            </TableRow>
          </TableHead>
          <TableBody>
            {sensors.length === 0 ? (
              <TableRow>
                <TableCell colSpan={4}>
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    align="center"
                  >
                    No public sensors configured
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              sensors.map((sensor) => {
                const sourceSensor = allSensors.find(
                  (s) => s.identifier === sensor.parentIdentifier,
                );

                return (
                  <TableRow key={sensor.identifier}>
                    <TableCell>{sensor.name}</TableCell>
                    <TableCell>
                      {sourceSensor?.name ?? sensor.parentIdentifier}
                    </TableCell>
                    <TableCell>
                      {sensor.measurementType !== undefined
                        ? getMeasurementUnit(
                            sensor.measurementType as MeasurementTypes,
                          )
                        : "-"}
                    </TableCell>
                    {(onEdit || onDelete) && (
                      <TableCell align="right">
                        <Box display="flex" justifyContent="flex-end" gap={0.5}>
                          {onEdit && (
                            <IconButton
                              size="small"
                              onClick={() => onEdit(sensor)}
                              title="Edit public sensor"
                            >
                              <Edit fontSize="small" />
                            </IconButton>
                          )}
                          {onDelete && (
                            <IconButton
                              size="small"
                              onClick={() => onDelete(sensor)}
                              title="Delete public sensor"
                              color="error"
                            >
                              <Delete fontSize="small" />
                            </IconButton>
                          )}
                        </Box>
                      </TableCell>
                    )}
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};
