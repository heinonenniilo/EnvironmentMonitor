import { type DeviceEvent } from "../models/deviceEvent";
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
import { getFormattedDate } from "../utilities/datetimeUtils";

export interface DeviceEventTableProps {
  events: DeviceEvent[];
  title?: string;
  maxHeight?: string;
}

export const DeviceEventTable: React.FC<DeviceEventTableProps> = ({
  events,
  title,
  maxHeight,
}) => {
  return (
    <Box marginTop={2} display={"flex"} flexDirection={"column"} flexGrow={"1"}>
      {title !== undefined ? (
        <Typography variant="h6" marginBottom={2}>
          {title}
        </Typography>
      ) : null}
      <TableContainer
        component={Paper}
        sx={{ overflow: "auto", maxHeight: "400px" }}
      >
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Type</TableCell>
              <TableCell>Message</TableCell>
              <TableCell>Timestamp</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {events.map((event, idx) => (
              <TableRow
                key={event.id}
                sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
              >
                <TableCell>{event.type}</TableCell>
                <TableCell>{event.message}</TableCell>
                <TableCell>{getFormattedDate(event.timeStamp, true)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};
