import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
} from "@mui/material";
import React, { useEffect, useState } from "react";
import moment from "moment";
import { DesktopDatePicker } from "@mui/x-date-pickers/DesktopDatePicker";
import { type Sensor } from "../models/sensor";
import { stringSort } from "../utilities/stringUtils";
import { getEntityTitle } from "../utilities/entityUtils";
import { type Entity } from "../models/entity";
import {
  getMeasurementTypeDisplayName,
  getMeasurementUnit,
} from "../utilities/measurementUtils";
import { MeasurementTypes } from "../enums/measurementTypes";

export interface MeasurementsLeftViewProps {
  onSearch: (
    from: moment.Moment,
    to: moment.Moment | undefined,
    sensorIds: string[],
    measurementTypes?: number[]
  ) => void;
  onSelectEntity: (deviceId: string) => void;
  toggleSensorSelection: (sensorId: string) => void;
  entities: Entity[];
  sensors: Sensor[];
  selectedSensors: string[];
  selectedEntities: Entity[] | undefined;
  timeFrom?: moment.Moment;
  timeTo?: moment.Moment;
  entityName?: string;
  selectedMeasurementTypes: number[];
  onMeasurementTypesChange: (measurementTypes: number[]) => void;
}

export const MeasurementsLeftView: React.FC<MeasurementsLeftViewProps> = ({
  onSearch,
  entities,
  sensors,
  onSelectEntity,
  toggleSensorSelection,
  selectedSensors,
  selectedEntities,
  timeFrom,
  timeTo,
  entityName,
  selectedMeasurementTypes,
  onMeasurementTypesChange,
}) => {
  const [fromDate, setFromDate] = useState<moment.Moment>(
    moment().utc(true).add(-2, "day").startOf("day")
  );
  const [toDate, setToDate] = useState<moment.Moment | undefined>(undefined);

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

  useEffect(() => {
    if (timeFrom) {
      setFromDate(timeFrom);
    }
  }, [timeFrom]);

  useEffect(() => {
    setToDate(timeTo);
  }, [timeTo]);

  const getSensorText = (sensor: Sensor) => {
    const measurementUnit =
      sensor.measurementType !== undefined
        ? getMeasurementUnit(sensor.measurementType)
        : undefined;
    if (selectedEntities && selectedEntities.length > 1) {
      const matchingEntity = entities.find(
        (d) => d.identifier === sensor.parentIdentifier
      );

      const letToReturn = `${
        matchingEntity?.displayName ?? matchingEntity?.name
      }: ${sensor.name}`;

      if (measurementUnit) {
        return `${letToReturn} (${measurementUnit})`;
      }
      return `${matchingEntity?.displayName ?? matchingEntity?.name}: ${
        sensor.name
      }`;
    }
    if (measurementUnit) {
      return `${sensor.name} (${measurementUnit})`;
    }
    return sensor.name;
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
          value={fromDate}
          onChange={(value) => {
            if (value) {
              setFromDate(value.utc(true).startOf("day"));
            }
          }}
        ></DesktopDatePicker>
      </Box>

      <Box mt={2}>
        <DesktopDatePicker
          label="To"
          format="DD.MM.YYYY"
          value={toDate}
          slotProps={{ field: { clearable: true } }}
          onChange={(value) => {
            if (value) {
              setToDate(value.utc(true).endOf("day"));
            } else {
              setToDate(undefined);
            }
          }}
        />
      </Box>
      <Box mt={2}>
        <FormControl fullWidth>
          <InputLabel id="device-select-label">
            {entityName ?? "Device"}
          </InputLabel>
          <Select
            labelId="device-select-label"
            id="device-select"
            value={
              selectedEntities
                ? selectedEntities.map((s) => {
                    return s.identifier;
                  })
                : []
            }
            label={entityName ?? "Device"}
            multiple
          >
            {[...entities]
              .sort((a, b) => stringSort(getEntityTitle(a), getEntityTitle(b)))
              .map((y) => (
                <MenuItem
                  value={y.identifier}
                  key={`device-${y.identifier}`}
                  onClick={() => {
                    onSelectEntity(y.identifier);
                  }}
                >
                  {getEntityTitle(y)}
                </MenuItem>
              ))}
          </Select>
        </FormControl>
      </Box>
      <Box mt={2}>
        <FormControl fullWidth>
          <InputLabel id="device-select-label">Sensor</InputLabel>
          <Select
            labelId="sensor-select-label"
            id="sensor-select"
            value={selectedSensors}
            multiple
            label="Sensor"
          >
            {[...sensors]
              .sort((a, b) => stringSort(getSensorText(a), getSensorText(b)))
              .map((y) => (
                <MenuItem
                  value={y.identifier}
                  key={`sensor-${y.identifier}`}
                  onClick={() => {
                    toggleSensorSelection(y.identifier);
                  }}
                >
                  {getSensorText(y)}
                </MenuItem>
              ))}
          </Select>
        </FormControl>
      </Box>

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

      <Box mt={2}>
        <Button
          variant="outlined"
          onClick={() => {
            if (selectedSensors.length > 0) {
              onSearch(
                fromDate,
                toDate,
                selectedSensors,
                selectedMeasurementTypes.length > 0
                  ? selectedMeasurementTypes
                  : undefined
              );
            }
          }}
        >
          Search
        </Button>
      </Box>
    </Box>
  );
};
