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
                  getMeasurementTypeDisplayName(b as MeasurementTypes),
                ),
              )
              .map((type) => (
                <MenuItem
                  value={type}
                  key={`measurement-type-${type}`}
                  onClick={() => {
                    handleToggleMeasurementType(type);
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
