import {
  Box,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
} from "@mui/material";
import React from "react";
import { MeasurementTypes } from "../../enums/measurementTypes";
import {
  getAvailableMeasurementTypes,
  getMeasurementTypeDisplayName,
} from "../../utilities/measurementUtils";
import { stringSort } from "../../utilities/stringUtils";
import { Clear } from "@mui/icons-material";
import { type LocationModel } from "../../models/location";

export interface DashboardLeftMenuProps {
  selectedMeasurementTypes: number[];
  onMeasurementTypesChange: (measurementTypes: number[]) => void;
  locations?: LocationModel[];
  selectedLocationIdentifiers?: string[] | null;
  onLocationFilterChange?: (locationIdentifiers: string[] | null) => void;
}

export const DashboardLeftMenu: React.FC<DashboardLeftMenuProps> = ({
  selectedMeasurementTypes,
  onMeasurementTypesChange,
  locations,
  selectedLocationIdentifiers,
  onLocationFilterChange,
}) => {
  const availableMeasurementTypes = getAvailableMeasurementTypes();

  const handleToggleMeasurementType = (type: number) => {
    if (selectedMeasurementTypes.includes(type)) {
      onMeasurementTypesChange(
        selectedMeasurementTypes.filter((t) => t !== type),
      );
    } else {
      onMeasurementTypesChange([...selectedMeasurementTypes, type]);
    }
  };

  const handleClear = () => {
    onMeasurementTypesChange([]);
  };

  const showLocationFilter =
    locations !== undefined && onLocationFilterChange !== undefined;

  const handleToggleLocation = (identifier: string) => {
    if (!onLocationFilterChange) return;
    if (
      selectedLocationIdentifiers === null ||
      selectedLocationIdentifiers === undefined
    ) {
      onLocationFilterChange([identifier]);
    } else if (selectedLocationIdentifiers.includes(identifier)) {
      const updated = selectedLocationIdentifiers.filter(
        (id) => id !== identifier,
      );
      onLocationFilterChange(updated.length > 0 ? updated : null);
    } else {
      onLocationFilterChange([...selectedLocationIdentifiers, identifier]);
    }
  };

  const handleClearLocations = () => {
    if (!onLocationFilterChange) return;
    onLocationFilterChange(null);
  };

  const optionsToShow = availableMeasurementTypes.map((type) => ({
    value: type,
    label: getMeasurementTypeDisplayName(type as MeasurementTypes, true),
  }));

  const locationOptions = showLocationFilter
    ? [...locations]
        .sort((a, b) =>
          stringSort(a.displayName ?? a.name, b.displayName ?? b.name),
        )
        .map((loc) => ({
          value: loc.identifier,
          label: loc.displayName ?? loc.name,
        }))
    : [];

  return (
    <Box
      display={"flex"}
      flexDirection={"column"}
      width={"100%"}
      justifyContent={"flex-start"}
      minWidth={250}
    >
      <Box mt={2}>
        <FormControl fullWidth>
          <InputLabel id="measurement-type-select-label">
            Measurement Type
          </InputLabel>
          <Select
            labelId="measurement-type-select-label"
            id="measurement-type-select"
            value={selectedMeasurementTypes}
            multiple
            label="Measurement Type"
            endAdornment={
              selectedMeasurementTypes.length > 0 ? (
                <IconButton
                  size="small"
                  onClick={handleClear}
                  sx={{ marginRight: 3 }}
                >
                  <Clear fontSize="small" />
                </IconButton>
              ) : null
            }
          >
            {optionsToShow
              .sort((a, b) => stringSort(a.label, b.label))
              .map((option) => (
                <MenuItem
                  value={option.value}
                  key={`measurement-type-${option.value}`}
                  onClick={() => {
                    handleToggleMeasurementType(option.value);
                  }}
                >
                  {option.label}
                </MenuItem>
              ))}
          </Select>
        </FormControl>
      </Box>
      {showLocationFilter && (
        <Box mt={2}>
          <FormControl fullWidth>
            <InputLabel id="location-select-label">Location</InputLabel>
            <Select
              labelId="location-select-label"
              id="location-select"
              value={selectedLocationIdentifiers ?? []}
              multiple
              label="Location"
              endAdornment={
                selectedLocationIdentifiers != null &&
                selectedLocationIdentifiers.length > 0 ? (
                  <IconButton
                    size="small"
                    onClick={handleClearLocations}
                    sx={{ marginRight: 3 }}
                  >
                    <Clear fontSize="small" />
                  </IconButton>
                ) : null
              }
            >
              {locationOptions.map((option) => (
                <MenuItem
                  value={option.value}
                  key={`location-${option.value}`}
                  onClick={() => {
                    handleToggleLocation(option.value);
                  }}
                >
                  {option.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>
      )}
    </Box>
  );
};
