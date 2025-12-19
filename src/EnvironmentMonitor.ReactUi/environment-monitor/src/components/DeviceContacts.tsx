import type { DeviceContact } from "../models/deviceContact";
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
  IconButton,
} from "@mui/material";
import { getFormattedDate } from "../utilities/datetimeUtils";
import { Edit, Delete } from "@mui/icons-material";

export interface DeviceContactsProps {
  contacts: DeviceContact[];
  title?: string;
  onUpdate?: (contact: DeviceContact) => void;
  onDelete?: (contact: DeviceContact) => void;
}

export const DeviceContacts: React.FC<DeviceContactsProps> = ({
  title,
  contacts,
  onUpdate,
  onDelete,
}) => {
  const showActions = onUpdate || onDelete;

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
              <TableCell>Email</TableCell>
              <TableCell>Created</TableCell>
              {showActions && <TableCell align="right">Actions</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {contacts.map((contact) => {
              return (
                <TableRow key={contact.identifier}>
                  <TableCell>{contact.email}</TableCell>
                  <TableCell>
                    {contact.created
                      ? getFormattedDate(contact.created, true, true)
                      : ""}
                  </TableCell>
                  {showActions && (
                    <TableCell align="right">
                      <Box display="flex" justifyContent="flex-end" gap={0.5}>
                        {onUpdate && (
                          <IconButton
                            size="small"
                            onClick={() => onUpdate(contact)}
                            title="Edit contact"
                          >
                            <Edit fontSize="small" />
                          </IconButton>
                        )}
                        {onDelete && (
                          <IconButton
                            size="small"
                            onClick={() => onDelete(contact)}
                            title="Delete contact"
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
            })}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};
