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
                Enable motion control
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
              <MenuItem onClick={() => handleClickDelayItem(10000)}>
                Motion control delays 10 S
              </MenuItem>
              <MenuItem onClick={() => handleClickDelayItem(45000)}>
                Motion control delays 45 S
              </MenuItem>
              <MenuItem onClick={() => handleClickDelayItem(60000)}>
                Motion control delays 1 min
              </MenuItem>
              <MenuItem onClick={() => handleClickDelayItem(120000)}>
                Motion control delays 2 min
              </MenuItem>
              <MenuItem onClick={() => handleClickDelayItem(240000)}>
                Motion control delays 4 min
              </MenuItem>
            </Menu>
          </>
        ) : null}
      </Box>
    </Box>
  );
};
