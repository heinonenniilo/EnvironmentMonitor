import type { DeviceAttribute } from "../models/deviceAttribute";
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

export interface DeviceAttributesTableProps {
  attributes: DeviceAttribute[];
  title?: string;
}

export const DeviceAttributesTable: React.FC<DeviceAttributesTableProps> = ({
  title,
  attributes,
}) => {
  return (
    <Box marginTop={1}>
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
              <TableCell>Description</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Value</TableCell>
              <TableCell>Updated</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {attributes.map((attr, index) => {
              return (
                <TableRow key={`${attr.typeName}-${index}`}>
                  <TableCell>{attr.typeName}</TableCell>
                  <TableCell>{attr.typeDescription}</TableCell>
                  <TableCell>{attr.type}</TableCell>
                  <TableCell>{attr.value}</TableCell>
                  <TableCell>
                    {attr.timeStamp
                      ? getFormattedDate(attr.timeStamp, true, true)
                      : ""}
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};
