import {
  Box,
  Button,
  Menu,
  MenuItem,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import { DeviceInfo } from "../models/deviceInfo";
import { useState } from "react";

export interface DeviceControlComponentProps {
  reboot: () => void;
  onSetOutStatic: (on: boolean) => void;
  onSetOutOnMotionControl: () => void;
  onSetMotionControlDelay: (delay: number) => void;
  device: DeviceInfo | undefined;
  title?: string;
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

  const handleClickDelayItem = (delay: number) => {
    onSetMotionControlDelay(delay);
    setAnchorOutputDelay(null);
  };

  const handleClickSetOutputMode = (functionToCall: () => void) => {
    functionToCall();
    setAnchorOutputMode(null);
  };
  const [anchorOutputDelay, setAnchorOutputDelay] =
    useState<null | HTMLElement>(null);

  const [anchorOutputMode, setAnchorOutputMode] = useState<null | HTMLElement>(
    null
  );

  return (
    <Box marginTop={2}>
      {title !== undefined ? (
        <Typography variant="h6" marginBottom={2}>
          {title}
        </Typography>
      ) : null}
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
                  handleClickSetOutputMode(() => onSetOutStatic(true));
                }}
              >
                Set output ON (motion control OFF)
              </MenuItem>
              <MenuItem
                onClick={() => {
                  handleClickSetOutputMode(() => onSetOutStatic(false));
                }}
              >
                Set output OFF (motion control OFF)
              </MenuItem>
              <MenuItem
                onClick={() =>
                  handleClickSetOutputMode(() => onSetOutOnMotionControl())
                }
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
    </Box>
  );
};
