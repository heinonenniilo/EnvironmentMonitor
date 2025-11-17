import { type DeviceQueuedCommandDto } from "../models/deviceQueuedCommand";
import {
  Box,
  Typography,
  Chip,
  Dialog,
  DialogContent,
  DialogTitle,
  IconButton,
} from "@mui/material";
import { getFormattedDate } from "../utilities/datetimeUtils";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useState } from "react";
import { Close, Delete } from "@mui/icons-material";

export interface DeviceQueuedCommandsTableProps {
  commands: DeviceQueuedCommandDto[];
  title?: string;
  maxHeight?: string;
  onDelete?: (messageId: string) => void;
}

export const DeviceQueuedCommandsTable: React.FC<
  DeviceQueuedCommandsTableProps
> = ({ commands, title, maxHeight, onDelete }) => {
  const [selectedMessage, setSelectedMessage] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);

  const formatDate = (input: Date | undefined | null) => {
    if (input) {
      return getFormattedDate(input, true, true);
    } else {
      return "-";
    }
  };

  const handleMessageClick = (message: string) => {
    if (message && message.trim()) {
      setSelectedMessage(message);
      setDialogOpen(true);
    }
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setSelectedMessage(null);
  };

  const formatJsonMessage = (message: string) => {
    try {
      const parsed = JSON.parse(message);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return message;
    }
  };

  const columns: GridColDef[] = [
    {
      field: "type",
      headerName: "Type",
      flex: 1,
      minWidth: 80,
    },
    {
      field: "message",
      headerName: "Message",
      flex: 2,
      minWidth: 200,
      renderCell: (params) => {
        const message = params.value as string;
        return (
          <Box
            onClick={() => handleMessageClick(message)}
            sx={{
              cursor: message && message.trim() ? "pointer" : "default",
              color: message && message.trim() ? "primary.main" : "inherit",
              textDecoration: message && message.trim() ? "underline" : "none",
              "&:hover": {
                color: message && message.trim() ? "primary.dark" : "inherit",
              },
              overflow: "hidden",
              textOverflow: "ellipsis",
              whiteSpace: "nowrap",
            }}
          >
            {message || "-"}
          </Box>
        );
      },
    },
    {
      field: "scheduled",
      headerName: "Scheduled",
      type: "dateTime",
      flex: 1,
      minWidth: 170,
      valueGetter: (_value, row) => {
        if (row) {
          const data = row as DeviceQueuedCommandDto;
          if (data.scheduled) {
            return new Date(data.scheduled);
          }
        }
        return null;
      },
      valueFormatter: (_value, row) => {
        return formatDate((row as DeviceQueuedCommandDto)?.scheduled);
      },
    },
    {
      field: "executedAt",
      headerName: "Executed At",
      type: "dateTime",
      flex: 1,
      minWidth: 170,
      valueGetter: (_value, row) => {
        if (row) {
          const data = row as DeviceQueuedCommandDto;
          if (data.executedAt) {
            return new Date(data.executedAt);
          }
        }
        return null;
      },
      valueFormatter: (_value, row) => {
        return formatDate((row as DeviceQueuedCommandDto)?.executedAt);
      },
    },
    {
      field: "status",
      headerName: "Status",
      flex: 1,
      minWidth: 120,
      sortable: false,
      renderCell: (params) => {
        const command = params.row as DeviceQueuedCommandDto;
        if (command.isRemoved) {
          return <Chip label="Cancelled" color="error" size="small" />;
        } else if (command.executedAt) {
          return <Chip label="Executed" color="success" size="small" />;
        } else {
          return <Chip label="Pending" color="warning" size="small" />;
        }
      },
      valueGetter: (_value, row) => {
        const command = row as DeviceQueuedCommandDto;
        if (command.isRemoved) {
          return "Cancelled";
        }
        return command.executedAt ? "Executed" : "Pending";
      },
    },
    {
      field: "created",
      headerName: "Created",
      type: "dateTime",
      flex: 1,
      minWidth: 170,
      valueGetter: (_value, row) => {
        if (row) {
          const data = row as DeviceQueuedCommandDto;
          if (data.created) {
            return new Date(data.created);
          }
        }
        return null;
      },
      valueFormatter: (_value, row) => {
        return formatDate((row as DeviceQueuedCommandDto)?.created);
      },
    },
    {
      field: "actions",
      headerName: "Actions",
      sortable: false,
      filterable: false,
      width: 80,
      renderCell: (params) => {
        const command = params.row as DeviceQueuedCommandDto;
        // Only show delete button for pending commands (not executed or cancelled)
        if (command.executedAt || command.isRemoved || !onDelete) {
          return null;
        }
        return (
          <IconButton
            onClick={() => onDelete(command.messageId)}
            size="small"
            color="error"
            aria-label="delete"
          >
            <Delete />
          </IconButton>
        );
      },
    },
  ];

  return (
    <>
      <Box
        marginTop={2}
        display={"flex"}
        flexDirection={"column"}
        flexGrow={"1"}
      >
        {title !== undefined ? (
          <Typography variant="h6" marginBottom={2}>
            {title}
          </Typography>
        ) : null}
        <Box sx={{ overflow: "auto", maxHeight: maxHeight ?? "400px" }}>
          <DataGrid
            rows={commands}
            columns={columns}
            getRowId={(row) => {
              if (row) {
                return (row as DeviceQueuedCommandDto).messageId;
              }
              return "";
            }}
            sx={{
              border: 0,
              minWidth: 600,
            }}
            pagination={undefined}
            pageSizeOptions={[]}
            hideFooter
            initialState={{
              sorting: {
                sortModel: [
                  {
                    field: "scheduled",
                    sort: "desc",
                  },
                ],
              },
            }}
          />
        </Box>
      </Box>

      <Dialog
        open={dialogOpen}
        onClose={handleCloseDialog}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle
          sx={{
            display: "flex",
            flexDirection: "row",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Box>Command Message</Box>
          <IconButton
            aria-label="close"
            onClick={handleCloseDialog}
            sx={{
              color: (theme) => theme.palette.grey[500],
            }}
            size="small"
          >
            <Close />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          <Box
            component="pre"
            sx={{
              backgroundColor: (theme) => theme.palette.grey[100],
              padding: 2,
              borderRadius: 1,
              overflow: "auto",
              maxHeight: "60vh",
              fontFamily: "monospace",
              fontSize: "0.875rem",
              whiteSpace: "pre-wrap",
              wordBreak: "break-word",
            }}
          >
            {selectedMessage ? formatJsonMessage(selectedMessage) : ""}
          </Box>
        </DialogContent>
      </Dialog>
    </>
  );
};
