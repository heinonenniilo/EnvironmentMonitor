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
import { Device } from "../models/device";
import { Sensor } from "../models/sensor";

export interface MeasurementsInfoTableProps {
  infoRows: MeasurementInfo[];
  hideMin?: boolean;
  hideMax?: boolean;
}

export interface MeasurementInfo {
  min: Measurement;
  max: Measurement;
  latest: Measurement;
  label: string;
  device?: Device;
  sensor?: Sensor;
}

export const MeasurementsInfoTable: React.FC<MeasurementsInfoTableProps> = ({
  infoRows,
  hideMax,
  hideMin,
}) => {
  const getLabel = (row: MeasurementInfo) => {
    return <TableCell>{row.label}</TableCell>;
  };

  const getDeviceLabel = (row: MeasurementInfo) => {
    if (!row.device) {
      return null;
    }
    return <TableCell>{`${row.device.name}`}</TableCell>;
  };

  const hasDevices = () => {
    return infoRows?.some((r) => r.device !== undefined);
  };
  return (
    <TableContainer component={Paper}>
      <Table size="small" aria-label="a dense table">
        <TableHead>
          <TableRow>
            {hasDevices() ? <TableCell>Device</TableCell> : null}
            <TableCell>Sensor</TableCell>
            {hideMin ? null : <TableCell>Min</TableCell>}

            {hideMax ? null : <TableCell>Max</TableCell>}
            <TableCell>Latest</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {infoRows.map((r) => {
            return (
              <TableRow
                sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
              >
                {getDeviceLabel(r)}
                {getLabel(r)}
                {hideMin ? null : (
                  <TableCell>{formatMeasurement(r.min)}</TableCell>
                )}
                {hideMax ? null : (
                  <TableCell>{formatMeasurement(r.max)}</TableCell>
                )}

                <TableCell>{formatMeasurement(r.latest)}</TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
};
