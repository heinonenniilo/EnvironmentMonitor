import { AppContentWrapper } from "../framework/AppContentWrapper";
import {
  Button,
  Checkbox,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from "@mui/material";
import { ConfirmationDialog } from "../framework/ConfirmationDialog";
import { useEffect, useState } from "react";
import { Device } from "../models/device";
import { useApiHook } from "../hooks/apiHook";
import { DeviceInfo } from "../models/deviceInfo";
import { getFormattedDate } from "../utilities/datetimeUtils";

export const DevicesView: React.FC = () => {
  const [selectedDevice, setSelectedDevice] = useState<Device | undefined>(
    undefined
  );

  const [isLoading, setIsLoading] = useState(false);
  const deviceHook = useApiHook().deviceHook;

  const [deviceInfos, setDeviceInfos] = useState<DeviceInfo[]>([]);

  const getDialogTitle = () => {
    return `Reboot ${selectedDevice?.name}?`;
  };

  const getDialogBody = () => {
    if (!selectedDevice) {
      return "";
    }

    return `Name: ${selectedDevice.name}, Identifier: ${selectedDevice?.deviceIdentifier}, Id: ${selectedDevice.id}?`;
  };

  useEffect(() => {
    if (deviceInfos.length === 0 && deviceHook) {
      getDevices();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [deviceInfos]);

  const getDevices = () => {
    setIsLoading(true);
    deviceHook
      .getDeviceInfos()
      .then((res) => {
        if (res) {
          setDeviceInfos(res);
        }
      })
      .catch((er) => {
        console.error(er);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const rebootDevice = () => {
    if (!selectedDevice) {
      return;
    }
    setIsLoading(true);
    const deviceIdentifier = selectedDevice.deviceIdentifier;
    setSelectedDevice(undefined);

    deviceHook
      .rebootDevice(deviceIdentifier)
      .then((r) => {
        if (r) {
          alert("Message sent to device");
          getDevices();
        } else {
          alert("Sending the message failed!");
        }
      })
      .catch((er) => {
        alert("Sending message failed");
      })
      .finally(() => {
        setIsLoading(false);
        setSelectedDevice(undefined);
      });
    //
  };
  return (
    <AppContentWrapper titleParts={[{ text: "Devices" }]} isLoading={isLoading}>
      <ConfirmationDialog
        isOpen={selectedDevice !== undefined}
        onClose={() => {
          setSelectedDevice(undefined);
        }}
        onConfirm={rebootDevice}
        title={getDialogTitle()}
        body={getDialogBody()}
      />
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Reboot</TableCell>
              <TableCell>Visible</TableCell>
              <TableCell>Online Since</TableCell>
              <TableCell>Rebooted On</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {deviceInfos.map((info) => (
              <TableRow
                key={info.device.id}
                sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
              >
                <TableCell>{info.device.name}</TableCell>
                <TableCell>
                  <Button
                    variant="contained"
                    onClick={() => {
                      setSelectedDevice(info.device);
                    }}
                  >
                    Reboot
                  </Button>
                </TableCell>
                <TableCell>
                  <Checkbox
                    checked={info.device.visible}
                    size="small"
                    disabled
                    sx={{ padding: "0px" }}
                  />
                </TableCell>
                <TableCell>
                  {info.onlineSince
                    ? getFormattedDate(info.onlineSince, true)
                    : ""}
                </TableCell>
                <TableCell>
                  {info.rebootedOn
                    ? getFormattedDate(info.rebootedOn, true)
                    : ""}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </AppContentWrapper>
  );
};
