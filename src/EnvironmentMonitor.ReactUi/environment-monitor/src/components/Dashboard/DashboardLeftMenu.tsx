import { Box, FormControl, InputLabel, MenuItem, Select } from "@mui/material";
import React from "react";
import { MeasurementTypes } from "../../enums/measurementTypes";
import { getMeasurementTypeDisplayName } from "../../utilities/measurementUtils";
import { stringSort } from "../../utilities/stringUtils";

export interface DashboardLeftMenuProps {
  selectedMeasurementTypes: number[];
  onMeasurementTypesChange: (measurementTypes: number[]) => void;
}

export const DashboardLeftMenu: React.FC<DashboardLeftMenuProps> = ({
  selectedMeasurementTypes,
  onMeasurementTypesChange,
}) => {
  // Get all available measurement types from enum (excluding Undefined and Online)
  const availableMeasurementTypes = Object.keys(MeasurementTypes)
    .filter(
      (key) =>
        !isNaN(Number(MeasurementTypes[key as keyof typeof MeasurementTypes]))
    )
    .map((key) =>
      Number(MeasurementTypes[key as keyof typeof MeasurementTypes])
    )
    .filter(
      (value) =>
        value !== MeasurementTypes.Undefined &&
        value !== MeasurementTypes.Online
    );

  const toggleMeasurementTypeSelection = (type: number) => {
    let newSelection: number[];
    if (selectedMeasurementTypes.includes(type)) {
      newSelection = selectedMeasurementTypes.filter((t) => t !== type);
    } else {
      newSelection = [...selectedMeasurementTypes, type];
    }
    onMeasurementTypesChange(newSelection);
  };

  return (
    <Box
      display={"flex"}
      flexDirection={"column"}
      width={"100%"}
      justifyContent={"flex-start"}
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
          >
            {availableMeasurementTypes
              .sort((a, b) =>
                stringSort(
                  getMeasurementTypeDisplayName(a as MeasurementTypes),
                  getMeasurementTypeDisplayName(b as MeasurementTypes)
                )
              )
              .map((type) => (
                <MenuItem
                  value={type}
                  key={`measurement-type-${type}`}
                  onClick={() => {
                    toggleMeasurementTypeSelection(type);
                  }}
                >
                  {getMeasurementTypeDisplayName(type as MeasurementTypes)}
                </MenuItem>
              ))}
          </Select>
        </FormControl>
      </Box>
    </Box>
  );
};
