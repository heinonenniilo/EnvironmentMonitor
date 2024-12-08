import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useSelector } from "react-redux";
import { getDevices } from "../reducers/measurementReducer";
import { Button, List, ListItem, ListItemText } from "@mui/material";
import { ConfirmationDialog } from "../framework/ConfirmationDialog";
import { useState } from "react";
import { Device } from "../models/device";
import { useApiHook } from "../hooks/apiHook";

export const DevicesView: React.FC = () => {
  const devices = useSelector(getDevices);
  const [selectedDevice, setSelectedDevice] = useState<Device | undefined>(
    undefined
  );

  const [isLoading, setIsLoading] = useState(false);
  const deviceHook = useApiHook().deviceHook;

  const getDialogTitle = () => {
    return `Reboot ${selectedDevice?.name}?`;
  };

  const getDialogBody = () => {
    if (!selectedDevice) {
      return "";
    }

    return `Name: ${selectedDevice.name}, Identifier: ${selectedDevice?.deviceIdentifier}, Id: ${selectedDevice.id}?`;
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
      <List>
        {devices.map((device) => (
          <ListItem
            key={device.id}
            sx={{ display: "flex", flexDirection: "row", maxWidth: "400px" }}
            style={{
              display: "flex",
              flexDirection: "row",
              alignItems: "center",
            }}
          >
            <ListItemText primary={device.name} />
            <Button
              variant="contained"
              onClick={() => {
                setSelectedDevice(device);
              }}
            >
              Reboot
            </Button>
          </ListItem>
        ))}
      </List>
    </AppContentWrapper>
  );
};
