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

  const handleClear = () => {
    onMeasurementTypesChange([]);
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
    </Box>
  );
};
