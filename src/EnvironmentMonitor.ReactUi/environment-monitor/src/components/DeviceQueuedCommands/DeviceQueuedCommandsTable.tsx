import { type DeviceQueuedCommandDto } from "../../models/deviceQueuedCommand";
import { Box, Typography, Chip, IconButton, Tooltip } from "@mui/material";
import { getFormattedDate } from "../../utilities/datetimeUtils";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useState } from "react";
import { Delete, Schedule, Visibility, ContentCopy } from "@mui/icons-material";
import moment from "moment";
import { EditQueuedCommandDialog } from "./EditQueuedCommandDialog";

export interface DeviceQueuedCommandsTableProps {
  commands: DeviceQueuedCommandDto[];
  title?: string;
  maxHeight?: string;
  onDelete?: (messageId: string) => void;
  onChangeScheduledTime?: (
    messageId: string,
    deviceIdentifier: string,
    newScheduledTime: moment.Moment
  ) => void;
  onCopy?: (messageId: string, deviceIdentifier: string) => void;
}

export const DeviceQueuedCommandsTable: React.FC<
  DeviceQueuedCommandsTableProps
> = ({
  commands,
  title,
  maxHeight,
  onDelete,
  onChangeScheduledTime,
  onCopy,
}) => {
  const [dialogState, setDialogState] = useState<{
    command: DeviceQueuedCommandDto | null;
    viewOnly: boolean;
  }>({ command: null, viewOnly: false });

  const formatDate = (input: Date | undefined | null) => {
    if (input) {
      return getFormattedDate(input, true, true);
    } else {
      return "-";
    }
  };

  const handleCloseDialog = () => {
    setDialogState({ command: null, viewOnly: false });
  };

  const handleConfirmEdit = (
    messageId: string,
    deviceIdentifier: string,
    newScheduledTime: moment.Moment
  ) => {
    if (onChangeScheduledTime) {
      onChangeScheduledTime(messageId, deviceIdentifier, newScheduledTime);
    }
    handleCloseDialog();
  };

  const columns: GridColDef[] = [
    {
      field: "type",
      headerName: "Type",
      flex: 1,
      minWidth: 80,
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
      width: 120,
      renderCell: (params) => {
        const command = params.row as DeviceQueuedCommandDto;

        const showEditButtons = !command.executedAt && !command.isRemoved;
        const showCopyButton = command.executedAt && onCopy;
        return (
          <Box
            display="flex"
            gap={0.5}
            sx={{
              alignItems: "center",
              height: "100%",
            }}
          >
            <Tooltip title="View">
              <IconButton
                onClick={() => setDialogState({ command, viewOnly: true })}
                size="small"
                color="info"
                aria-label="view details"
              >
                <Visibility />
              </IconButton>
            </Tooltip>
            {showCopyButton && (
              <Tooltip title="Copy">
                <IconButton
                  onClick={() =>
                    onCopy(command.messageId, command.deviceIdentifier)
                  }
                  size="small"
                  color="secondary"
                  aria-label="copy command"
                >
                  <ContentCopy />
                </IconButton>
              </Tooltip>
            )}
            {showEditButtons && onChangeScheduledTime && (
              <Tooltip title="Edit Schedule">
                <IconButton
                  onClick={() => setDialogState({ command, viewOnly: false })}
                  size="small"
                  color="primary"
                  aria-label="edit schedule"
                >
                  <Schedule />
                </IconButton>
              </Tooltip>
            )}
            {showEditButtons && onDelete && (
              <Tooltip title="Cancel">
                <IconButton
                  onClick={() => onDelete(command.messageId)}
                  size="small"
                  color="error"
                  aria-label="delete"
                >
                  <Delete />
                </IconButton>
              </Tooltip>
            )}
          </Box>
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

      <EditQueuedCommandDialog
        open={dialogState.command !== null}
        command={dialogState.command}
        onClose={handleCloseDialog}
        onConfirm={handleConfirmEdit}
        viewOnly={dialogState.viewOnly}
      />
    </>
  );
};
