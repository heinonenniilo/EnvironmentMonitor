import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
  Typography,
  IconButton,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { type DeviceQueuedCommandDto } from "../models/deviceQueuedCommand";
import { useState, useEffect } from "react";
import { getFormattedDate } from "../utilities/datetimeUtils";
import moment from "moment";

export interface EditQueuedCommandDialogProps {
  open: boolean;
  command: DeviceQueuedCommandDto | null;
  onClose: () => void;
  onConfirm: (
    messageId: string,
    deviceIdentifier: string,
    newScheduledTime: moment.Moment
  ) => void;
}

export const EditQueuedCommandDialog: React.FC<
  EditQueuedCommandDialogProps
> = ({ open, command, onClose, onConfirm }) => {
  const [scheduledTime, setScheduledTime] = useState<string>("");

  useEffect(() => {
    if (command && command.scheduled) {
      // Format the date for datetime-local input
      const momentDate = moment(command.scheduled);
      const formattedDate = momentDate.format("YYYY-MM-DDTHH:mm");
      setScheduledTime(formattedDate);
    }
  }, [command]);

  const handleConfirm = () => {
    if (!command || !scheduledTime) {
      return;
    }
    console.log(command);

    const newMoment = moment(scheduledTime);
    onConfirm(command.messageId, command.deviceIdentifier, newMoment);
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
        <Box>Edit Queued Command</Box>
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
                maxHeight: "200px",
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
              Current Scheduled Time
            </Typography>
            <Typography variant="body1">
              {getFormattedDate(command.scheduled, true, true)}
            </Typography>
          </Box>

          <Box>
            <Typography variant="subtitle2" color="text.secondary" mb={1}>
              New Scheduled Time
            </Typography>
            <TextField
              type="datetime-local"
              value={scheduledTime}
              onChange={(e) => setScheduledTime(e.target.value)}
              fullWidth
              InputLabelProps={{
                shrink: true,
              }}
            />
          </Box>
        </Box>
      </DialogContent>
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
    </Dialog>
  );
};
