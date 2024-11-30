import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from "@mui/material";
import { Measurement } from "../models/measurement";
import { formatMeasurement } from "../utilities/measurementUtils";

export interface MeasurementsInfoTableProps {
  infoRows: MeasurementInfo[];
}

export interface MeasurementInfo {
  min: Measurement;
  max: Measurement;
  latest: Measurement;
  label: string;
}

export const MeasurementsInfoTable: React.FC<MeasurementsInfoTableProps> = ({
  infoRows,
}) => {
  return (
    <TableContainer component={Paper}>
      <Table size="small" aria-label="a dense table">
        <TableHead>
          <TableRow>
            <TableCell>Sensor</TableCell>
            <TableCell>Min</TableCell>
            <TableCell>Max</TableCell>
            <TableCell>Latest</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {infoRows.map((r) => {
            return (
              <TableRow
                sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
              >
                <TableCell>{r.label}</TableCell>
                <TableCell>{formatMeasurement(r.min)}</TableCell>
                <TableCell>{formatMeasurement(r.max)}</TableCell>
                <TableCell>{formatMeasurement(r.latest)}</TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
};
