import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
} from "@mui/material";
import React, { useEffect, useState } from "react";
import { DesktopDatePicker } from "@mui/x-date-pickers/DesktopDatePicker";
import type { GetDeviceMessagesModel } from "../models/getDeviceMessagesModel";
import { defaultStart } from "../containers/DeviceMessagesView";
import { stringSort } from "../utilities/stringUtils";
import { getEntityTitle } from "../utilities/entityUtils";
import type { LocationModel } from "../models/location";
import type { DeviceInfo } from "../models/deviceInfo";

export interface DeviceMessagesLeftViewProps {
  onSearch: (model: GetDeviceMessagesModel) => void;
  devices: DeviceInfo[];
  locations: LocationModel[];
  model: GetDeviceMessagesModel;
}

export const DeviceMessagesLeftView: React.FC<DeviceMessagesLeftViewProps> = ({
  onSearch,
  model,
  devices,
  locations,
}) => {
  const [innerModel, setModel] = useState<GetDeviceMessagesModel | undefined>(
    undefined
  );

  useEffect(() => {
    if (model) {
      setModel(model);
    }
  }, [model]);

  const fromValue = () => {
    if (innerModel) {
      return innerModel.from;
    }
    return defaultStart;
  };

  const toggleLocation = (locationIdentifier: string) => {
    if (!innerModel) {
      return;
    }
    const prevSelected = innerModel.locationIdentifiers?.some(
      (s) => s === locationIdentifier
    );
    if (prevSelected) {
      setModel({
        ...innerModel,
        locationIdentifiers: innerModel.locationIdentifiers?.filter(
          (s) => s !== locationIdentifier
        ),
        deviceIdentifiers: innerModel.deviceIdentifiers
          ? innerModel.deviceIdentifiers.filter((deviceId) => {
              const matchingDevice = devices.find(
                (d) => d.device.identifier === deviceId
              );
              return (
                matchingDevice?.device.locationIdentifier !== locationIdentifier
              );
            })
          : undefined,
      });
    } else {
      const locationIdsToSet = innerModel.locationIdentifiers
        ? [...innerModel.locationIdentifiers, locationIdentifier]
        : [locationIdentifier];

      const deviceIdsToSelect = devices
        .filter((d) => d.device.locationIdentifier === locationIdentifier)
        .map((d) => d.device.identifier);
      setModel({
        ...innerModel,
        locationIdentifiers: locationIdsToSet,
        deviceIdentifiers: innerModel.deviceIdentifiers
          ? [...innerModel.deviceIdentifiers, ...deviceIdsToSelect]
          : deviceIdsToSelect,
      });
    }
  };

  return (
    <Box
      display={"flex"}
      flexDirection={"column"}
      width={"100%"}
      justifyContent={"space-between"}
    >
      <Box mt={2}>
        <DesktopDatePicker
          label="From"
          format="DD.MM.YYYY"
          value={fromValue()}
          onChange={(value) => {
            if (value && innerModel) {
              setModel({ ...innerModel, from: value.utc(true).startOf("day") });
            }
          }}
        ></DesktopDatePicker>
      </Box>

      <Box mt={2}>
        <DesktopDatePicker
          label="To"
          format="DD.MM.YYYY"
          value={innerModel?.to}
          slotProps={{ field: { clearable: true } }}
          onChange={(value) => {
            if (innerModel) {
              setModel({
                ...innerModel,
                to: value ? value.utc(true).endOf("day") : undefined,
              });
            }
          }}
        />
      </Box>
      <Box mt={2}>
        <FormControl fullWidth>
          <InputLabel id="device-select-label">Location</InputLabel>
          <Select
            labelId="location-select-label"
            id="location-select"
            value={innerModel?.locationIdentifiers ?? []}
            label="Location"
            multiple
          >
            {[...locations]
              .sort((a, b) => stringSort(a.name, b.name))
              .map((y) => (
                <MenuItem
                  value={y.identifier}
                  key={`location-${y.identifier}`}
                  onClick={() => toggleLocation(y.identifier)}
                >
                  {y.name}
                </MenuItem>
              ))}
          </Select>
        </FormControl>
      </Box>
      <Box mt={2}>
        <FormControl fullWidth>
          <InputLabel id="device-select-label">Device</InputLabel>
          <Select
            labelId="device-select-label"
            id="device-select"
            value={innerModel?.deviceIdentifiers ?? []}
            label="Device"
            onChange={(event) => {
              if (innerModel) {
                const selectedDeviceIds = event.target.value as string[];
                setModel({
                  ...innerModel,
                  deviceIdentifiers: selectedDeviceIds,
                });
              }
            }}
            multiple
          >
            {[
              ...devices.filter((d) =>
                (innerModel?.locationIdentifiers ?? []).some(
                  (l) => d.device.locationIdentifier === l
                )
              ),
            ]
              .sort((a, b) =>
                stringSort(getEntityTitle(a.device), getEntityTitle(b.device))
              )
              .map((y) => (
                <MenuItem
                  value={y.device.identifier}
                  key={`device-${y.device.identifier}`}
                >
                  {getEntityTitle(y.device)}
                </MenuItem>
              ))}
          </Select>
        </FormControl>
      </Box>

      <Box mt={2}>
        <FormControl fullWidth size="small">
          <InputLabel id="is-duplicate-label">Is duplicate</InputLabel>
          <Select
            labelId="is-duplicate-label"
            value={
              innerModel === undefined || innerModel.isDuplicate === undefined
                ? "-1"
                : innerModel.isDuplicate
                ? "1"
                : "0"
            }
            label="Is duplicate"
            onChange={(event) => {
              if (!innerModel) {
                return;
              }
              const val = event.target.value;
              setModel({
                ...innerModel,
                isDuplicate: val === "-1" ? undefined : val === "1",
              });
            }}
          >
            <MenuItem value={"-1"}>No filter</MenuItem>
            <MenuItem value={"1"}>True</MenuItem>
            <MenuItem value={"0"}>False</MenuItem>
          </Select>
        </FormControl>
      </Box>

      <Box mt={2}>
        <FormControl fullWidth size="small">
          <InputLabel id="is-first-label">First Message</InputLabel>
          <Select
            labelId="is-first-label"
            value={
              innerModel === undefined ||
              innerModel.isFirstMessage === undefined
                ? "-1"
                : innerModel.isFirstMessage
                ? "1"
                : "0"
            }
            label="First Message"
            onChange={(event) => {
              if (!innerModel) {
                return;
              }
              const val = event.target.value;
              setModel({
                ...innerModel,
                isFirstMessage: val === "-1" ? undefined : val === "1",
              });
            }}
          >
            <MenuItem value={"-1"}>No filter</MenuItem>
            <MenuItem value={"1"}>True</MenuItem>
            <MenuItem value={"0"}>False</MenuItem>
          </Select>
        </FormControl>
      </Box>
      <Box mt={2}>
        <Button
          variant="outlined"
          onClick={() => {
            if (innerModel) {
              onSearch(innerModel);
            }
          }}
        >
          Search
        </Button>
      </Box>
    </Box>
  );
};
