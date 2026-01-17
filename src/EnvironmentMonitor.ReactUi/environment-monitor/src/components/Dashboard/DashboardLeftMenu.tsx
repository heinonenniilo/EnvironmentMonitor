import { Box, FormControl, InputLabel, MenuItem, Select } from "@mui/material";
import React from "react";
import { MeasurementTypes } from "../../enums/measurementTypes";
import {
  getAvailableMeasurementTypes,
  getMeasurementTypeDisplayName,
} from "../../utilities/measurementUtils";
import { stringSort } from "../../utilities/stringUtils";

export interface DashboardLeftMenuProps {
  selectedMeasurementTypes: number[];
  onMeasurementTypesChange: (measurementTypes: number[]) => void;
}

export const DashboardLeftMenu: React.FC<DashboardLeftMenuProps> = ({
  selectedMeasurementTypes,
  onMeasurementTypesChange,
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

  const optionsToShow = availableMeasurementTypes.map((type) => ({
    value: type,
    label: getMeasurementTypeDisplayName(type as MeasurementTypes, true),
  }));

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
    </Box>
  );
};
