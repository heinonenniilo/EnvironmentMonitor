import { type SensorInfo, type VirtualSensor } from "../../models/sensor";
import {
  Box,
  Checkbox,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { SensorsDialog } from "./SensorsDialog";
import { useState } from "react";
import { getAggregationTypeDisplayName } from "../../utilities/measurementUtils";
import { Edit, Delete } from "@mui/icons-material";
import type { AddVirtualSensorRowDto } from "../../models/updateVirtualSensorRows";

export interface SensorTableProps {
  sensors: SensorInfo[];
  title?: string;
  isVirtual?: boolean;
  location?: string;
  onEdit?: (sensor: SensorInfo) => void;
  onDelete?: (sensor: SensorInfo) => void;
  onToggleActive?: (sensor: SensorInfo, isActive: boolean) => void;
  onUpdateVirtualSensorRows?: (
    sensor: SensorInfo,
    rowsToAdd: AddVirtualSensorRowDto[],
    rowsToDelete: string[],
  ) => Promise<void>;
}

export const SensorTable: React.FC<SensorTableProps> = ({
  title,
  sensors,
  isVirtual,
  location,
  onEdit,
  onDelete,
  onToggleActive,
  onUpdateVirtualSensorRows,
}) => {
  const [selectedParentSensor, setSelectedParentSensor] =
    useState<SensorInfo | null>(null);
  const [selectedSensors, setSelectedSensors] = useState<VirtualSensor[]>([]);
  const [dialogTitle, setDialogTitle] = useState<string>("");

  const canClickRow = (sensor: SensorInfo) => {
    return isVirtual || sensor.isVirtual || sensor.sensors.length > 0;
  };

  return (
    <Box marginTop={1}>
      {title && (
        <Typography variant="h6" marginBottom={2}>
          {title}
        </Typography>
      )}
      <SensorsDialog
        isOpen={selectedParentSensor !== null}
        onClose={() => {
          setSelectedParentSensor(null);
          setSelectedSensors([]);
          setDialogTitle("");
        }}
        sensors={selectedSensors}
        title={dialogTitle}
        location={location}
        editable={isVirtual || (selectedParentSensor?.isVirtual ?? false)}
        onSave={(rowsToAdd, rowsToDelete) => {
          if (!selectedParentSensor || !onUpdateVirtualSensorRows) {
            return Promise.resolve();
          }

          return onUpdateVirtualSensorRows(
            selectedParentSensor,
            rowsToAdd,
            rowsToDelete,
          );
        }}
      />
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Sensor Id</TableCell>
              <TableCell>Virtual</TableCell>
              <TableCell>Scale min</TableCell>
              <TableCell>Scale max</TableCell>
              <TableCell>Active</TableCell>
              {isVirtual && <TableCell>Aggregation Type</TableCell>}
              {(onEdit || onDelete) && <TableCell align="right"></TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {sensors
              .sort((a, b) => a.sensorId - b.sensorId)
              .map((r) => {
                return (
                  <TableRow
                    key={r.identifier}
                    onClick={() => {
                      if (canClickRow(r)) {
                        setSelectedParentSensor(r);
                        setSelectedSensors(r.sensors);
                        setDialogTitle(`Sensors for ${r.name}`);
                      }
                    }}
                    sx={{
                      cursor: canClickRow(r) ? "pointer" : "default",
                      "&:hover": canClickRow(r)
                        ? {
                            backgroundColor: "action.hover",
                          }
                        : undefined,
                    }}
                  >
                    <TableCell>{r.name}</TableCell>
                    <TableCell>{r.sensorId}</TableCell>
                    <TableCell>
                      <Checkbox
                        checked={r.isVirtual}
                        size="small"
                        disabled
                        sx={{
                          padding: "0px",
                        }}
                      />
                    </TableCell>
                    <TableCell>{r.scaleMin}</TableCell>
                    <TableCell>{r.scaleMax}</TableCell>
                    <TableCell>
                      <Checkbox
                        checked={r.active ?? false}
                        size="small"
                        disabled={!onToggleActive}
                        onChange={(e) => {
                          e.stopPropagation();
                          if (onToggleActive) {
                            onToggleActive(r, e.target.checked);
                          }
                        }}
                        onClick={(e) => e.stopPropagation()}
                        sx={{
                          padding: "0px",
                        }}
                      />
                    </TableCell>
                    {isVirtual && (
                      <TableCell>
                        {getAggregationTypeDisplayName(r.aggregationType)}
                      </TableCell>
                    )}
                    {(onEdit || onDelete) && (
                      <TableCell align="right">
                        <Box display="flex" justifyContent="flex-end" gap={0.5}>
                          {onEdit && (
                            <IconButton
                              size="small"
                              onClick={(e) => {
                                e.stopPropagation();
                                onEdit(r);
                              }}
                              title="Edit sensor"
                            >
                              <Edit fontSize="small" />
                            </IconButton>
                          )}
                          {onDelete && (
                            <IconButton
                              size="small"
                              onClick={(e) => {
                                e.stopPropagation();
                                onDelete(r);
                              }}
                              title="Delete sensor"
                              color="error"
                            >
                              <Delete fontSize="small" />
                            </IconButton>
                          )}
                        </Box>
                      </TableCell>
                    )}
                  </TableRow>
                );
              })}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};
