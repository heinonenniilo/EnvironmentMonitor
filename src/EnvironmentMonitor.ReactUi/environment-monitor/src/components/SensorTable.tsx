import { Sensor } from "../models/sensor";
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";

export interface SensorTableProps {
  sensors: Sensor[];
  title?: string;
}

export const SensorTable: React.FC<SensorTableProps> = ({ title, sensors }) => {
  return (
    <Box marginTop={2}>
      {title && (
        <Typography variant="h6" marginBottom={2}>
          {title}
        </Typography>
      )}

      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Sensor Id</TableCell>
              <TableCell>Scale min</TableCell>
              <TableCell>Scale max</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {sensors.map((r) => {
              return (
                <TableRow key={r.id}>
                  <TableCell>{r.name}</TableCell>
                  <TableCell>{r.sensorId}</TableCell>
                  <TableCell>{r.scaleMin}</TableCell>
                  <TableCell>{r.scaleMax}</TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};
