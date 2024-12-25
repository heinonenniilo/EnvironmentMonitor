import {
  Box,
  Button,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import { DeviceInfo } from "../models/deviceInfo";

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
              onClick={() => {
                onSetOutStatic(true);
              }}
            >
              Set output ON (motion control OFF)
            </Button>
            <Button
              variant="contained"
              color="primary"
              onClick={() => {
                onSetOutStatic(false);
              }}
            >
              Set output OFF (motion control OFF)
            </Button>
            <Button
              variant="contained"
              color="primary"
              onClick={onSetOutOnMotionControl}
            >
              Enable motion control
            </Button>
          </>
        ) : null}
      </Box>
      {hasMotionSensor ? (
        <Box
          display="flex"
          justifyContent="start"
          gap={2}
          marginTop={2}
          flexDirection={drawDesktop ? "row" : "column"}
        >
          <Button
            variant="contained"
            color="primary"
            onClick={() => {
              onSetMotionControlDelay(10000);
            }}
          >
            Motion control delays 10 S
          </Button>
          <Button
            variant="contained"
            color="primary"
            onClick={() => {
              onSetMotionControlDelay(45000);
            }}
          >
            Motion control delays 45 S
          </Button>
          <Button
            variant="contained"
            color="primary"
            onClick={() => {
              onSetMotionControlDelay(60000);
            }}
          >
            Motion control delays 1 min S
          </Button>
          <Button
            variant="contained"
            color="primary"
            onClick={() => {
              onSetMotionControlDelay(120000);
            }}
          >
            Motion control delays 2 min S
          </Button>
          <Button
            variant="contained"
            color="primary"
            onClick={() => {
              onSetMotionControlDelay(240000);
            }}
          >
            Motion control delays 4 min S
          </Button>
        </Box>
      ) : null}
    </Box>
  );
};
