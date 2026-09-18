import {
  Box,
  Button,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
} from "@mui/material";
import { Clear } from "@mui/icons-material";
import React, { useEffect, useState } from "react";
import { DesktopDatePicker } from "@mui/x-date-pickers/DesktopDatePicker";
import type { GetDeviceMessagesModel } from "../../models/getDeviceMessagesModel";
import { defaultStart } from "../../containers/DeviceMessagesView";
import { stringSort } from "../../utilities/stringUtils";
import { getEntityTitle } from "../../utilities/entityUtils";
import type { LocationModel } from "../../models/location";
import type { DeviceInfo } from "../../models/deviceInfo";
import {
  CommunicationChannels,
  getCommunicationChannelDisplayName,
} from "../../enums/communicationChannels";

const communicationChannelOptions = Object.values(CommunicationChannels).filter(
  (value): value is CommunicationChannels => typeof value === "number",
);

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
            endAdornment={
              (innerModel?.locationIdentifiers?.length ?? 0) > 0 ? (
                <IconButton
                  size="small"
                  onClick={() => {
                    if (innerModel) {
                      setModel({
                        ...innerModel,
                        locationIdentifiers: [],
                        deviceIdentifiers: [],
                      });
                    }
                  }}
                  sx={{ marginRight: 3 }}
                >
                  <Clear fontSize="small" />
                </IconButton>
              ) : null
            }
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
            endAdornment={
              (innerModel?.deviceIdentifiers?.length ?? 0) > 0 ? (
                <IconButton
                  size="small"
                  onClick={() => {
                    if (innerModel) {
                      setModel({ ...innerModel, deviceIdentifiers: [] });
                    }
                  }}
                  sx={{ marginRight: 3 }}
                >
                  <Clear fontSize="small" />
                </IconButton>
              ) : null
            }
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
        <FormControl fullWidth>
          <InputLabel id="source-select-label">Source</InputLabel>
          <Select
            labelId="source-select-label"
            id="source-select"
            value={innerModel?.sourceIds ?? []}
            label="Source"
            multiple
            onChange={(event) => {
              if (innerModel) {
                const sourceIds = event.target.value as number[];
                setModel({
                  ...innerModel,
                  sourceIds: sourceIds.length > 0 ? sourceIds : undefined,
                });
              }
            }}
            endAdornment={
              (innerModel?.sourceIds?.length ?? 0) > 0 ? (
                <IconButton
                  size="small"
                  onClick={() => {
                    if (innerModel) {
                      setModel({ ...innerModel, sourceIds: undefined });
                    }
                  }}
                  sx={{ marginRight: 3 }}
                >
                  <Clear fontSize="small" />
                </IconButton>
              ) : null
            }
          >
            {communicationChannelOptions.map((sourceId) => (
              <MenuItem value={sourceId} key={`source-${sourceId}`}>
                {getCommunicationChannelDisplayName(sourceId)}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      {([
        { field: "isDuplicate", label: "Is duplicate" },
        { field: "isFirstMessage", label: "First Message" },
      ] as const).map(({ field, label }) => (
        <Box mt={2} key={field}>
          <FormControl fullWidth>
            <InputLabel id={field + "-label"}>{label}</InputLabel>
            <Select<string[]>
              labelId={field + "-label"}
              id={field + "-select"}
              value={innerModel?.[field] == null ? [] : [String(innerModel[field])]}
              label={label}
              multiple
              onChange={(event) => {
                if (!innerModel) {
                  return;
                }
                const selected = typeof event.target.value === "string"
                  ? event.target.value.split(",")
                  : event.target.value;
                setModel({
                  ...innerModel,
                  [field]: selected.includes("null") || selected.length !== 1
                    ? null
                    : selected[0] === "true",
                });
              }}
              endAdornment={
                innerModel?.[field] != null ? (
                  <IconButton
                    size="small"
                    aria-label={"Clear " + label + " filter"}
                    onClick={() => {
                      if (innerModel) {
                        setModel({ ...innerModel, [field]: null });
                      }
                    }}
                    sx={{ marginRight: 3 }}
                  >
                    <Clear fontSize="small" />
                  </IconButton>
                ) : null
              }
            >
              <MenuItem value="null">No filter</MenuItem>
              <MenuItem value="true">True</MenuItem>
              <MenuItem value="false">False</MenuItem>
            </Select>
          </FormControl>
        </Box>
      ))}
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
