import { DeviceInfo } from "../models/deviceInfo";
import {
  Box,
  Button,
  Checkbox,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { routes } from "../utilities/routes";
import { Link } from "react-router";
import { getFormattedDate } from "../utilities/datetimeUtils";

export interface DeviceTableProps {
  devices: DeviceInfo[];
  showLink?: boolean;
  hideName?: boolean;
  onReboot?: (device: DeviceInfo) => void;
  title?: string;
}

export const DeviceTable: React.FC<DeviceTableProps> = ({
  devices,
  showLink,
  hideName,
  onReboot,
  title,
}) => {
  return (
    <Box marginTop={2}>
      {title !== undefined ? (
        <Typography variant="h6" marginBottom={2}>
          {title}
        </Typography>
      ) : null}
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              {!hideName ? <TableCell>Name</TableCell> : null}
              {onReboot !== undefined ? <TableCell>Reboot</TableCell> : null}
              <TableCell>Visible</TableCell>
              <TableCell>Online Since</TableCell>
              <TableCell>Rebooted On</TableCell>
              <TableCell>Last Message</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {devices.map((info) => (
              <TableRow
                key={info.device.id}
                sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
              >
                {!hideName ? (
                  showLink ? (
                    <TableCell>
                      <Link
                        to={`${routes.devices}/${info.device.deviceIdentifier}`}
                      >
                        {info.device.name}
                      </Link>
                    </TableCell>
                  ) : (
                    <TableCell>{info.device.name}</TableCell>
                  )
                ) : null}

                {onReboot !== undefined ? (
                  <TableCell>
                    <Button
                      variant="contained"
                      onClick={() => {
                        onReboot(info);
                      }}
                    >
                      Reboot
                    </Button>
                  </TableCell>
                ) : null}

                <TableCell>
                  <Checkbox
                    checked={info.device.visible}
                    size="small"
                    disabled
                    sx={{ padding: "0px" }}
                  />
                </TableCell>
                <TableCell>
                  {info.onlineSince
                    ? getFormattedDate(info.onlineSince, true)
                    : ""}
                </TableCell>
                <TableCell>
                  {info.rebootedOn
                    ? getFormattedDate(info.rebootedOn, true)
                    : ""}
                </TableCell>
                <TableCell>
                  {info.lastMessage
                    ? getFormattedDate(info.lastMessage, true)
                    : ""}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};
