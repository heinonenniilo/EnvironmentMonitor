import { Clear } from "@mui/icons-material";
import {
  Box,
  Button,
  FormControl,
  FormLabel,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  ToggleButton,
  ToggleButtonGroup,
  type SelectChangeEvent,
} from "@mui/material";
import {
  CommunicationChannels,
  getCommunicationChannelDisplayName,
} from "../../enums/communicationChannels";
import type { LocationModel } from "../../models/location";
import { stringSort } from "../../utilities/stringUtils";

export interface DevicesViewLeftMenuProps {
  locations: LocationModel[];
  selectedLocationIdentifiers: string[];
  selectedCommunicationChannelIds: number[];
  selectedIsVirtual?: boolean;
  onLocationIdentifiersChange: (identifiers: string[]) => void;
  onCommunicationChannelIdsChange: (ids: number[]) => void;
  onIsVirtualChange: (isVirtual: boolean | undefined) => void;
  onSearch: () => void;
}

const communicationChannelOptions = Object.values(CommunicationChannels)
  .filter((value): value is CommunicationChannels => typeof value === "number")
  .map((value) => ({
    value,
    label: getCommunicationChannelDisplayName(value),
  }))
  .sort((a, b) => stringSort(a.label, b.label));

export const DevicesViewLeftMenu: React.FC<DevicesViewLeftMenuProps> = ({
  locations,
  selectedLocationIdentifiers,
  selectedCommunicationChannelIds,
  selectedIsVirtual,
  onLocationIdentifiersChange,
  onCommunicationChannelIdsChange,
  onIsVirtualChange,
  onSearch,
}) => {
  const locationOptions = [...locations]
    .sort((a, b) =>
      stringSort(a.displayName ?? a.name, b.displayName ?? b.name),
    )
    .map((location) => ({
      value: location.identifier,
      label: location.displayName ?? location.name,
    }));

  const handleLocationChange = (event: SelectChangeEvent<string[]>) => {
    const value = event.target.value;
    onLocationIdentifiersChange(
      typeof value === "string" ? value.split(",") : value,
    );
  };

  const handleCommunicationChannelChange = (
    event: SelectChangeEvent<number[]>,
  ) => {
    const value = event.target.value;
    onCommunicationChannelIdsChange(
      (typeof value === "string" ? value.split(",") : value).map(Number),
    );
  };

  return (
    <Box display="flex" flexDirection="column" minWidth={250} width="100%">
      <Box mt={2}>
        <FormControl fullWidth>
          <InputLabel id="devices-location-select-label">Location</InputLabel>
          <Select
            labelId="devices-location-select-label"
            id="devices-location-select"
            multiple
            label="Location"
            value={selectedLocationIdentifiers}
            onChange={handleLocationChange}
            renderValue={(selected) =>
              selected
                .map(
                  (identifier) =>
                    locationOptions.find((option) => option.value === identifier)
                      ?.label ?? identifier,
                )
                .join(", ")
            }
            endAdornment={
              selectedLocationIdentifiers.length > 0 ? (
                <IconButton
                  aria-label="Clear location filters"
                  size="small"
                  onMouseDown={(event) => event.stopPropagation()}
                  onClick={() => onLocationIdentifiersChange([])}
                  sx={{ mr: 3 }}
                >
                  <Clear fontSize="small" />
                </IconButton>
              ) : null
            }
          >
            {locationOptions.map((option) => (
              <MenuItem key={option.value} value={option.value}>
                {option.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <Box mt={2}>
        <FormControl fullWidth>
          <InputLabel id="devices-communication-channel-select-label">
            Communication channel
          </InputLabel>
          <Select
            labelId="devices-communication-channel-select-label"
            id="devices-communication-channel-select"
            multiple
            label="Communication channel"
            value={selectedCommunicationChannelIds}
            onChange={handleCommunicationChannelChange}
            renderValue={(selected) =>
              selected
                .map((id) =>
                  getCommunicationChannelDisplayName(
                    id as CommunicationChannels,
                  ),
                )
                .join(", ")
            }
            endAdornment={
              selectedCommunicationChannelIds.length > 0 ? (
                <IconButton
                  aria-label="Clear communication channel filters"
                  size="small"
                  onMouseDown={(event) => event.stopPropagation()}
                  onClick={() => onCommunicationChannelIdsChange([])}
                  sx={{ mr: 3 }}
                >
                  <Clear fontSize="small" />
                </IconButton>
              ) : null
            }
          >
            {communicationChannelOptions.map((option) => (
              <MenuItem key={option.value} value={option.value}>
                {option.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <Box mt={2}>
        <FormControl fullWidth>
          <FormLabel id="devices-device-type-label">Device type</FormLabel>
          <ToggleButtonGroup
            aria-labelledby="devices-device-type-label"
            exclusive
            fullWidth
            size="small"
            value={
              selectedIsVirtual === undefined
                ? "all"
                : selectedIsVirtual
                  ? "virtual"
                  : "physical"
            }
            onChange={(_, value: "all" | "physical" | "virtual" | null) => {
              if (value === null) return;
              onIsVirtualChange(
                value === "all" ? undefined : value === "virtual",
              );
            }}
            sx={{ mt: 1 }}
          >
            <ToggleButton value="all">All</ToggleButton>
            <ToggleButton value="physical">Physical</ToggleButton>
            <ToggleButton value="virtual">Virtual</ToggleButton>
          </ToggleButtonGroup>
        </FormControl>
      </Box>

      <Box mt={2}>
        <Button variant="outlined" onClick={onSearch}>
          Search
        </Button>
      </Box>
    </Box>
  );
};
