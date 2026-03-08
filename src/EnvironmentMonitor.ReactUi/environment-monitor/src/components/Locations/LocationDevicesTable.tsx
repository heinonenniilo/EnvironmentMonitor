import {
  Checkbox,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from "@mui/material";
import { Link } from "react-router";
import type { Device } from "../../models/device";
import { routes } from "../../utilities/routes";
import { getEntityTitle } from "../../utilities/entityUtils";

export interface LocationDevicesTableProps {
  devices: Device[];
}

export const LocationDevicesTable: React.FC<LocationDevicesTableProps> = ({
  devices,
}) => {
  return (
    <TableContainer component={Paper}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            <TableCell>Identifier</TableCell>
            <TableCell>Visible</TableCell>
            <TableCell>Motion Sensor</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {devices.map((device) => (
            <TableRow key={device.identifier}>
              <TableCell>
                <Link to={`${routes.devices}/${device.identifier}`}>
                  {getEntityTitle(device)}
                </Link>
              </TableCell>
              <TableCell>{device.identifier}</TableCell>
              <TableCell>
                <Checkbox
                  checked={device.visible}
                  disabled
                  size="small"
                  sx={{ padding: 0 }}
                />
              </TableCell>
              <TableCell>
                <Checkbox
                  checked={device.hasMotionSensor}
                  disabled
                  size="small"
                  sx={{ padding: 0 }}
                />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};
