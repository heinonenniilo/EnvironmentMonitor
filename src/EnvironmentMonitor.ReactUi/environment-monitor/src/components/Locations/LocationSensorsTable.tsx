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
} from "@mui/material";
import { Delete, Edit } from "@mui/icons-material";
import type { Sensor } from "../../models/sensor";
import { getMeasurementTypeDisplayName } from "../../utilities/measurementUtils";
import { MeasurementTypes } from "../../enums/measurementTypes";

export interface LocationSensorsTableProps {
  sensors: Sensor[];
  onEdit?: (sensor: Sensor) => void;
  onDelete?: (sensor: Sensor) => void;
}

export const LocationSensorsTable: React.FC<LocationSensorsTableProps> = ({
  sensors,
  onEdit,
  onDelete,
}) => {
  return (
    <TableContainer component={Paper}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            <TableCell>Identifier</TableCell>
            <TableCell>Measurement Type</TableCell>
            {(onEdit || onDelete) && <TableCell align="right"></TableCell>}
          </TableRow>
        </TableHead>
        <TableBody>
          {sensors.map((sensor) => (
            <TableRow key={sensor.identifier}>
              <TableCell>{sensor.displayName ?? sensor.name}</TableCell>
              <TableCell>{sensor.identifier}</TableCell>
              <TableCell>
                {sensor.measurementType !== undefined &&
                sensor.measurementType !== null
                  ? getMeasurementTypeDisplayName(
                      sensor.measurementType as MeasurementTypes,
                      true,
                    )
                  : "-"}
              </TableCell>
              {(onEdit || onDelete) && (
                <TableCell align="right">
                  <Box display="flex" justifyContent="flex-end" gap={0.5}>
                    {onEdit && (
                      <IconButton
                        size="small"
                        title="Edit location sensor"
                        onClick={() => onEdit(sensor)}
                      >
                        <Edit fontSize="small" />
                      </IconButton>
                    )}
                    {onDelete && (
                      <IconButton
                        size="small"
                        color="error"
                        title="Delete location sensor"
                        onClick={() => onDelete(sensor)}
                      >
                        <Delete fontSize="small" />
                      </IconButton>
                    )}
                  </Box>
                </TableCell>
              )}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};
