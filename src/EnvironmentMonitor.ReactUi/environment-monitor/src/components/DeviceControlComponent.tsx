import {
  Box,
  Button,
  Menu,
  MenuItem,
  Typography,
  useMediaQuery,
  useTheme,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControlLabel,
  Checkbox,
} from "@mui/material";
import { DateTimePicker } from "@mui/x-date-pickers/DateTimePicker";
import moment, { type Moment } from "moment";
import { type DeviceInfo } from "../models/deviceInfo";
import { useState } from "react";

export interface DeviceControlComponentProps {
  reboot: () => void;
  onSetOutStatic: (on: boolean, executeAt?: Moment) => void;
  onSetOutOnMotionControl: (executeAt?: Moment) => void;
  onSetMotionControlDelay: (delay: number, executeAt?: Moment) => void;
  device: DeviceInfo | undefined;
  title?: string;
}

interface CommandDialogState {
  open: boolean;
  title: string;
  description: string;
  onConfirm: (executeAt?: Moment) => void;
}

const delayOptions = [
  { delay: 10 },
  { delay: 45 },
  { delay: 60 },
  { delay: 2 * 60 },
  { delay: 5 * 60 },
  { delay: 10 * 60 },
  { delay: 15 * 60 },
  { delay: 20 * 60 },
];

export const DeviceControlComponent: React.FC<DeviceControlComponentProps> = ({
  onSetOutStatic,
  onSetOutOnMotionControl,
  onSetMotionControlDelay,
  device,
  reboot,
  title,
}) => {
  const theme = useTheme();
  const drawDesktop = useMediaQuery(theme.breakpoints.up("lg"));

  const hasMotionSensor = device?.device.hasMotionSensor ?? false;

  const [commandDialog, setCommandDialog] = useState<CommandDialogState>({
    open: false,
    title: "",
    description: "",
    onConfirm: () => {},
  });

  const [selectedDateTime, setSelectedDateTime] = useState<Moment | null>(
    moment()
  );
  const [useScheduledTime, setUseScheduledTime] = useState(false);

  const openCommandDialog = (
    title: string,
    description: string,
    onConfirm: (executeAt?: Moment) => void
  ) => {
    setCommandDialog({ open: true, title, description, onConfirm });
    setSelectedDateTime(moment());
    setUseScheduledTime(false);
    setAnchorOutputMode(null);
    setAnchorOutputDelay(null);
  };

  const handleDialogConfirm = () => {
    commandDialog.onConfirm(
      useScheduledTime && selectedDateTime ? selectedDateTime : undefined
    );
    setCommandDialog({ ...commandDialog, open: false });
  };

  const handleDialogClose = () => {
    setCommandDialog({ ...commandDialog, open: false });
  };

  const handleClickDelayItem = (delay: number) => {
    openCommandDialog(
      "Set motion control delay",
      `Motion control delay will be set to ${
        delay <= 60 ? `${delay} s` : `${delay / 60} min`
      }`,
      (executeAt) => onSetMotionControlDelay(delay, executeAt)
    );
  };

  const [anchorOutputDelay, setAnchorOutputDelay] =
    useState<null | HTMLElement>(null);

  const [anchorOutputMode, setAnchorOutputMode] = useState<null | HTMLElement>(
    null
  );

  return (
    <Box marginTop={2}>
      {title && (
        <Typography variant="h6" marginBottom={2}>
          {title}
        </Typography>
      )}
      <Box
        display="flex"
        justifyContent="start"
        gap={2}
        marginTop={2}
        flexDirection={drawDesktop ? "row" : "column"}
      >
        <Button variant="contained" color="primary" onClick={reboot}>
          Reboot
        </Button>
        {hasMotionSensor ? (
          <>
            <Button
              variant="contained"
              color="primary"
              onClick={(event) => {
                setAnchorOutputMode(event.currentTarget);
              }}
            >
              Set output mode
            </Button>
            <Menu
              anchorEl={anchorOutputMode}
              open={Boolean(anchorOutputMode)}
              onClose={() => {
                setAnchorOutputMode(null);
              }}
            >
              <MenuItem
                onClick={() => {
                  openCommandDialog(
                    "Set output ON",
                    "Output pins will be set as ON. Motion sensor trigger will be disabled",
                    (executeAt) => onSetOutStatic(true, executeAt)
                  );
                }}
              >
                Set output ON (motion control OFF)
              </MenuItem>
              <MenuItem
                onClick={() => {
                  openCommandDialog(
                    "Set output OFF",
                    "Output pins will be set as OFF. Motion sensor trigger will be disabled",
                    (executeAt) => onSetOutStatic(false, executeAt)
                  );
                }}
              >
                Set output OFF (motion control OFF)
              </MenuItem>
              <MenuItem
                onClick={() => {
                  openCommandDialog(
                    "Enable motion control",
                    "Output pins will be controlled by motion sensor",
                    (executeAt) => onSetOutOnMotionControl(executeAt)
                  );
                }}
              >
                Set motion control ON
              </MenuItem>
            </Menu>
            <Button
              variant="contained"
              color="primary"
              onClick={(event) => {
                setAnchorOutputDelay(event.currentTarget);
              }}
            >
              Set output delay
            </Button>

            <Menu
              anchorEl={anchorOutputDelay}
              open={Boolean(anchorOutputDelay)}
              onClose={() => {
                setAnchorOutputDelay(null);
              }}
            >
              {delayOptions.map((d, id) => (
                <MenuItem
                  onClick={() => {
                    handleClickDelayItem(d.delay);
                  }}
                  id={`md_${id}`}
                >
                  {d.delay <= 60 ? `${d.delay} s` : `${d.delay / 60} min`}
                </MenuItem>
              ))}
            </Menu>
          </>
        ) : null}
      </Box>

      <Dialog
        open={commandDialog.open}
        onClose={handleDialogClose}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>{commandDialog.title}</DialogTitle>
        <DialogContent>
          <Typography>{commandDialog.description}</Typography>
          <Box marginTop={3}>
            <FormControlLabel
              control={
                <Checkbox
                  checked={useScheduledTime}
                  onChange={(e) => setUseScheduledTime(e.target.checked)}
                />
              }
              label="Schedule for specific time"
            />
          </Box>
          {useScheduledTime && (
            <Box marginTop={2}>
              <DateTimePicker
                label="Execute at"
                value={selectedDateTime}
                onChange={(newValue) => setSelectedDateTime(newValue)}
                sx={{ width: "100%" }}
                format="DD.MM.YYYY HH:mm"
              />
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDialogClose}>Cancel</Button>
          <Button
            onClick={handleDialogConfirm}
            variant="contained"
            color="primary"
          >
            Confirm
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};
