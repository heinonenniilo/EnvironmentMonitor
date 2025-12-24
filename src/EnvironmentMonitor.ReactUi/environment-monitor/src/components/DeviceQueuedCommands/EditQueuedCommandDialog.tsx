import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  IconButton,
  Chip,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { useState, useEffect } from "react";
import moment, { type Moment } from "moment";
import { DateTimePicker } from "@mui/x-date-pickers/DateTimePicker";
import { getFormattedDate } from "../../utilities/datetimeUtils";
import type { DeviceQueuedCommandDto } from "../../models/deviceQueuedCommand";

export interface EditQueuedCommandDialogProps {
  open: boolean;
  command: DeviceQueuedCommandDto | null;
  onClose: () => void;
  onConfirm?: (
    messageId: string,
    deviceIdentifier: string,
    newScheduledTime: moment.Moment
  ) => void;
  viewOnly?: boolean;
}

export const EditQueuedCommandDialog: React.FC<
  EditQueuedCommandDialogProps
> = ({ open, command, onClose, onConfirm, viewOnly = false }) => {
  const [scheduledTime, setScheduledTime] = useState<Moment | null>(null);

  useEffect(() => {
    if (command && command.scheduled) {
      // Format the date for datetime-local input
      const momentDate = moment(command.scheduled);
      setScheduledTime(momentDate);
    }
  }, [command]);

  const handleConfirm = () => {
    if (!command || !scheduledTime || !onConfirm) {
      return;
    }

    onConfirm(command.messageId, command.deviceIdentifier, scheduledTime);
  };

  const formatJsonMessage = (message: string) => {
    try {
      const parsed = JSON.parse(message);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return message;
    }
  };

  if (!command) {
    return null;
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle
        sx={{
          display: "flex",
          flexDirection: "row",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Box display="flex" alignItems="center" gap={1.5}>
          <Box>{viewOnly ? "Command Message" : "Edit Queued Command"}</Box>
          {command.isRemoved ? (
            <Chip label="Cancelled" color="error" size="small" />
          ) : command.executedAt ? (
            <Chip label="Executed" color="success" size="small" />
          ) : (
            <Chip label="Pending" color="warning" size="small" />
          )}
        </Box>
        <IconButton
          aria-label="close"
          onClick={onClose}
          sx={{
            color: (theme) => theme.palette.grey[500],
          }}
          size="small"
        >
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <Box display="flex" flexDirection="column" gap={2} mt={1}>
          <Box>
            <Typography variant="subtitle2" color="text.secondary">
              Type
            </Typography>
            <Typography variant="body1">{command.type}</Typography>
          </Box>

          <Box>
            <Typography variant="subtitle2" color="text.secondary">
              Message
            </Typography>
            <Box
              component="pre"
              sx={{
                backgroundColor: (theme) => theme.palette.grey[100],
                padding: 2,
                borderRadius: 1,
                overflow: "auto",
                maxHeight: viewOnly ? "60vh" : "200px",
                fontFamily: "monospace",
                fontSize: "0.875rem",
                whiteSpace: "pre-wrap",
                wordBreak: "break-word",
                margin: 0,
                mt: 1,
              }}
            >
              {formatJsonMessage(command.message)}
            </Box>
          </Box>

          <Box>
            <Typography variant="subtitle2" color="text.secondary">
              {viewOnly ? "Scheduled Time" : "Current Scheduled Time"}
            </Typography>
            <Typography variant="body1">
              {getFormattedDate(command.scheduled, true, true)}
            </Typography>
          </Box>

          {command.executedAt && (
            <Box>
              <Typography variant="subtitle2" color="text.secondary">
                Executed At
              </Typography>
              <Typography variant="body1">
                {getFormattedDate(command.executedAt, true, true)}
              </Typography>
            </Box>
          )}

          {!viewOnly && (
            <Box>
              <Typography variant="subtitle2" color="text.secondary" mb={1}>
                New Scheduled Time
              </Typography>
              <DateTimePicker
                label="Execute at"
                value={scheduledTime}
                onChange={(newValue) => setScheduledTime(newValue)}
                sx={{ width: "100%" }}
                format="DD.MM.YYYY HH:mm"
              />
            </Box>
          )}
        </Box>
      </DialogContent>
      {!viewOnly && (
        <DialogActions>
          <Button onClick={onClose} color="inherit">
            Cancel
          </Button>
          <Button
            onClick={handleConfirm}
            color="primary"
            variant="contained"
            disabled={!scheduledTime}
          >
            Confirm
          </Button>
        </DialogActions>
      )}
    </Dialog>
  );
};
