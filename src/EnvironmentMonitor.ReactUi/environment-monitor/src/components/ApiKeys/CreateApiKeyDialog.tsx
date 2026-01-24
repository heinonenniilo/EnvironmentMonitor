import React, { useState, useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
  Typography,
  Alert,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  List,
  ListItem,
  ListItemText,
  IconButton,
  Paper,
  Divider,
} from "@mui/material";
import { Close, Add } from "@mui/icons-material";
import type { DeviceInfo } from "../../models/deviceInfo";
import type { LocationModel } from "../../models/location";
import { getClaimDisplayValue } from "../../utilities/claimUtils";

interface CreateApiKeyDialogProps {
  open: boolean;
  onClose: () => void;
  onCreate: (
    description: string,
    deviceIds: string[],
    locationIds: string[],
  ) => void;
  devices: DeviceInfo[];
  locations: LocationModel[];
  isLoading?: boolean;
}

export const CreateApiKeyDialog: React.FC<CreateApiKeyDialogProps> = ({
  open,
  onClose,
  onCreate,
  devices,
  locations,
  isLoading,
}) => {
  const [description, setDescription] = useState("");
  const [selectedDeviceIds, setSelectedDeviceIds] = useState<string[]>([]);
  const [selectedLocationIds, setSelectedLocationIds] = useState<string[]>([]);
  const [deviceToAdd, setDeviceToAdd] = useState("");
  const [locationToAdd, setLocationToAdd] = useState("");

  useEffect(() => {
    if (!open) {
      // Reset form when dialog closes
      setDescription("");
      setSelectedDeviceIds([]);
      setSelectedLocationIds([]);
      setDeviceToAdd("");
      setLocationToAdd("");
    }
  }, [open]);

  const handleCreate = () => {
    if (description.trim()) {
      onCreate(description.trim(), selectedDeviceIds, selectedLocationIds);
    }
  };

  const handleAddDevice = () => {
    if (deviceToAdd && !selectedDeviceIds.includes(deviceToAdd)) {
      setSelectedDeviceIds([...selectedDeviceIds, deviceToAdd]);
      setDeviceToAdd("");
    }
  };

  const handleRemoveDevice = (deviceId: string) => {
    setSelectedDeviceIds(selectedDeviceIds.filter((id) => id !== deviceId));
  };

  const handleAddLocation = () => {
    if (locationToAdd && !selectedLocationIds.includes(locationToAdd)) {
      setSelectedLocationIds([...selectedLocationIds, locationToAdd]);
      setLocationToAdd("");
    }
  };

  const handleRemoveLocation = (locationId: string) => {
    setSelectedLocationIds(
      selectedLocationIds.filter((id) => id !== locationId),
    );
  };

  const getDeviceName = (deviceId: string): string => {
    const device = devices.find((d) => d.device.identifier === deviceId);
    if (device) {
      return getClaimDisplayValue(
        { type: "DeviceId", value: deviceId },
        locations,
        devices.map((d) => d.device),
      );
    }
    return deviceId;
  };

  const getLocationName = (locationId: string): string => {
    return getClaimDisplayValue(
      { type: "LocationId", value: locationId },
      locations,
      devices.map((d) => d.device),
    );
  };

  const availableDevices = devices
    .filter((d) => !selectedDeviceIds.includes(d.device.identifier))
    .sort((a, b) => {
      const nameA = (a.device.displayName || a.device.name).toLowerCase();
      const nameB = (b.device.displayName || b.device.name).toLowerCase();
      return nameA.localeCompare(nameB);
    });

  const availableLocations = locations
    .filter((l) => !selectedLocationIds.includes(l.identifier))
    .sort((a, b) => a.name.toLowerCase().localeCompare(b.name.toLowerCase()));

  const canCreate =
    description.trim() &&
    (selectedDeviceIds.length > 0 || selectedLocationIds.length > 0);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>Create New API Key</DialogTitle>
      <DialogContent>
        <Box sx={{ mt: 2 }}>
          <Alert severity="info" sx={{ mb: 3 }}>
            <Typography variant="body2">
              API keys provide access to devices and locations. Select at least
              one device or location to grant access to.
            </Typography>
          </Alert>

          <TextField
            autoFocus
            fullWidth
            label="Description *"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Enter a description for this API key"
            helperText="Required: Provide a meaningful description to identify this API key"
            disabled={isLoading}
            sx={{ mb: 3 }}
          />

          {/* Add Device Section */}
          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle1" gutterBottom>
              Devices
            </Typography>
            <Box sx={{ display: "flex", gap: 1, mb: 2 }}>
              <FormControl fullWidth size="small">
                <InputLabel id="add-device-label">Add Device</InputLabel>
                <Select
                  labelId="add-device-label"
                  value={deviceToAdd}
                  onChange={(e) => setDeviceToAdd(e.target.value)}
                  label="Add Device"
                  disabled={isLoading || availableDevices.length === 0}
                >
                  {availableDevices.map((device) => (
                    <MenuItem
                      key={device.device.identifier}
                      value={device.device.identifier}
                    >
                      {device.device.displayName || device.device.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Button
                variant="contained"
                onClick={handleAddDevice}
                disabled={!deviceToAdd || isLoading}
                startIcon={<Add />}
                sx={{ minWidth: "100px" }}
              >
                Add
              </Button>
            </Box>

            {selectedDeviceIds.length > 0 ? (
              <Paper
                variant="outlined"
                sx={{ maxHeight: 200, overflow: "auto" }}
              >
                <List dense>
                  {[...selectedDeviceIds]
                    .sort((a, b) => {
                      const nameA = getDeviceName(a).toLowerCase();
                      const nameB = getDeviceName(b).toLowerCase();
                      return nameA.localeCompare(nameB);
                    })
                    .map((deviceId, index) => (
                      <React.Fragment key={deviceId}>
                        {index > 0 && <Divider />}
                        <ListItem
                          secondaryAction={
                            <IconButton
                              edge="end"
                              size="small"
                              onClick={() => handleRemoveDevice(deviceId)}
                              disabled={isLoading}
                            >
                              <Close />
                            </IconButton>
                          }
                        >
                          <ListItemText
                            primary={getDeviceName(deviceId)}
                            secondary={deviceId}
                            secondaryTypographyProps={{
                              sx: {
                                fontFamily: "monospace",
                                fontSize: "0.75rem",
                              },
                            }}
                          />
                        </ListItem>
                      </React.Fragment>
                    ))}
                </List>
              </Paper>
            ) : (
              <Box
                sx={{
                  p: 2,
                  textAlign: "center",
                  border: "1px dashed",
                  borderColor: "divider",
                  borderRadius: 1,
                }}
              >
                <Typography variant="body2" color="text.secondary">
                  No devices selected
                </Typography>
              </Box>
            )}
          </Box>

          {/* Add Location Section */}
          <Box sx={{ mb: 2 }}>
            <Typography variant="subtitle1" gutterBottom>
              Locations
            </Typography>
            <Box sx={{ display: "flex", gap: 1, mb: 2 }}>
              <FormControl fullWidth size="small">
                <InputLabel id="add-location-label">Add Location</InputLabel>
                <Select
                  labelId="add-location-label"
                  value={locationToAdd}
                  onChange={(e) => setLocationToAdd(e.target.value)}
                  label="Add Location"
                  disabled={isLoading || availableLocations.length === 0}
                >
                  {availableLocations.map((location) => (
                    <MenuItem
                      key={location.identifier}
                      value={location.identifier}
                    >
                      {location.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Button
                variant="contained"
                onClick={handleAddLocation}
                disabled={!locationToAdd || isLoading}
                startIcon={<Add />}
                sx={{ minWidth: "100px" }}
              >
                Add
              </Button>
            </Box>

            {selectedLocationIds.length > 0 ? (
              <Paper
                variant="outlined"
                sx={{ maxHeight: 200, overflow: "auto" }}
              >
                <List dense>
                  {[...selectedLocationIds]
                    .sort((a, b) => {
                      const nameA = getLocationName(a).toLowerCase();
                      const nameB = getLocationName(b).toLowerCase();
                      return nameA.localeCompare(nameB);
                    })
                    .map((locationId, index) => (
                      <React.Fragment key={locationId}>
                        {index > 0 && <Divider />}
                        <ListItem
                          secondaryAction={
                            <IconButton
                              edge="end"
                              size="small"
                              onClick={() => handleRemoveLocation(locationId)}
                              disabled={isLoading}
                            >
                              <Close />
                            </IconButton>
                          }
                        >
                          <ListItemText
                            primary={getLocationName(locationId)}
                            secondary={locationId}
                            secondaryTypographyProps={{
                              sx: {
                                fontFamily: "monospace",
                                fontSize: "0.75rem",
                              },
                            }}
                          />
                        </ListItem>
                      </React.Fragment>
                    ))}
                </List>
              </Paper>
            ) : (
              <Box
                sx={{
                  p: 2,
                  textAlign: "center",
                  border: "1px dashed",
                  borderColor: "divider",
                  borderRadius: 1,
                }}
              >
                <Typography variant="body2" color="text.secondary">
                  No locations selected
                </Typography>
              </Box>
            )}
          </Box>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={isLoading}>
          Cancel
        </Button>
        <Button
          onClick={handleCreate}
          variant="contained"
          disabled={!canCreate || isLoading}
        >
          Create API Key
        </Button>
      </DialogActions>
    </Dialog>
  );
};
