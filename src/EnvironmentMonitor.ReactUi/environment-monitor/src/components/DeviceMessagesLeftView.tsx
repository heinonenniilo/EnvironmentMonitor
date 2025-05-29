import {
  Box,
  Button,
  Checkbox,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Select,
} from "@mui/material";
import React, { useEffect, useState } from "react";
import { DesktopDatePicker } from "@mui/x-date-pickers/DesktopDatePicker";
import type { GetDeviceMessagesModel } from "../models/getDeviceMessagesModel";
import { defaultStart } from "../containers/DeviceMessagesView";
import type { Device } from "../models/device";
import { stringSort } from "../utilities/stringUtils";
import { getDeviceTitle } from "../utilities/deviceUtils";

export interface DeviceMessagesLeftViewProps {
  onSearch: (model: GetDeviceMessagesModel) => void;
  devices: Device[];
  model: GetDeviceMessagesModel;
}

export const DeviceMessagesLeftView: React.FC<DeviceMessagesLeftViewProps> = ({
  onSearch,
  model,
  devices,
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
          <InputLabel id="device-select-label">Device</InputLabel>
          <Select
            labelId="device-select-label"
            id="device-select"
            value={innerModel?.deviceIds ?? []}
            label="Device"
            onChange={(event) => {
              if (innerModel) {
                const selectedDeviceIds = event.target.value as number[];
                setModel({
                  ...innerModel,
                  deviceIds: selectedDeviceIds,
                });
              }
            }}
            multiple
          >
            {[...devices]
              .sort((a, b) => stringSort(getDeviceTitle(a), getDeviceTitle(b)))
              .map((y) => (
                <MenuItem value={y.id} key={`device-${y.id}`}>
                  {getDeviceTitle(y)}
                </MenuItem>
              ))}
          </Select>
        </FormControl>
      </Box>
      <Box mt={2}>
        <FormControlLabel
          control={
            <Checkbox
              checked={innerModel?.isDuplicate ?? false}
              onChange={(event) => {
                if (innerModel) {
                  setModel({
                    ...innerModel,
                    isDuplicate: event.target.checked,
                  });
                }
              }}
            />
          }
          label="Is duplicate"
        />
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
