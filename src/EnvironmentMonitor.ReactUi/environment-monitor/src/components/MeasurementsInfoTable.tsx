import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from "@mui/material";
import { formatMeasurement } from "../utilities/measurementUtils";
import { getFormattedDate } from "../utilities/datetimeUtils";
import type { Measurement } from "../models/measurement";
import type { Device } from "../models/device";
import type { Sensor } from "../models/sensor";
import type { MeasurementTypes } from "../enums/measurementTypes";
import { Link } from "react-router";
import { routes } from "../utilities/routes";

export interface MeasurementsInfoTableProps {
  infoRows: MeasurementInfo[];
  hideMin?: boolean;
  hideMax?: boolean;
  onClick?: (info: MeasurementInfo) => void;
  onHover?: (info: MeasurementInfo | null) => void;
  showSeconds?: boolean;
}

export interface MeasurementInfo {
  min: Measurement;
  max: Measurement;
  latest: Measurement;
  label: string;
  device?: Device;
  sensor?: Sensor;
  type?: MeasurementTypes;
  boldDevice?: boolean;
  hideDevice?: boolean;
  renderLinkToDevice?: boolean;
}

export const MeasurementsInfoTable: React.FC<MeasurementsInfoTableProps> = ({
  infoRows,
  hideMax,
  hideMin,
  onClick,
  onHover,
  showSeconds,
}) => {
  const getLabel = (row: MeasurementInfo) => {
    return (
      <TableCell
        onClick={
          onClick
            ? () => {
                onClick(row);
              }
            : undefined
        }
        sx={{
          cursor: onClick ? "pointer" : undefined,
          "&:hover": onClick
            ? {
                backgroundColor: "action.hover",
                transition: "background-color 0.2s ease",
              }
            : undefined,
        }}
      >
        {row.label}
      </TableCell>
    );
  };

  const getDeviceLabel = (row: MeasurementInfo) => {
    if (!row.device) {
      return null;
    }
    const label = row.hideDevice ? "" : row.device.displayName;
    const labelElement = row.boldDevice ? <b>{label}</b> : label;

    return (
      <TableCell>
        {row.renderLinkToDevice ? (
          <Link to={`${routes.measurements}/${row.device.identifier}`}>
            {labelElement}
          </Link>
        ) : (
          labelElement
        )}
      </TableCell>
    );
  };

  const hasDevices = () => {
    return infoRows?.some((r) => r.device !== undefined);
  };

  const showOnlyLatest = hideMin && hideMax;
  return (
    <TableContainer component={Paper} sx={{ backgroundColor: "inherit" }}>
      <Table size="small" aria-label="a dense table">
        <TableHead>
          <TableRow>
            {hasDevices() ? <TableCell>Device</TableCell> : null}
            <TableCell>Sensor</TableCell>
            {hideMin ? null : <TableCell>Min</TableCell>}
            {hideMax ? null : <TableCell>Max</TableCell>}
            <TableCell>Latest</TableCell>
            {showOnlyLatest ? <TableCell>Timestamp</TableCell> : null}
          </TableRow>
        </TableHead>
        <TableBody>
          {infoRows.map((r, idx) => {
            return (
              <TableRow
                sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
                key={`tablerow_${idx}`}
                onMouseEnter={() => onHover?.(r)}
                onMouseLeave={() => onHover?.(null)}
              >
                {getDeviceLabel(r)}
                {getLabel(r)}
                {hideMin ? null : (
                  <TableCell>{formatMeasurement(r.min)}</TableCell>
                )}
                {hideMax ? null : (
                  <TableCell>{formatMeasurement(r.max)}</TableCell>
                )}

                <TableCell>
                  {formatMeasurement(r.latest, showOnlyLatest)}
                </TableCell>
                {showOnlyLatest ? (
                  <TableCell>
                    {getFormattedDate(r.latest.timestamp, false, showSeconds)}
                  </TableCell>
                ) : null}
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
};
