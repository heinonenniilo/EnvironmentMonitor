import { type SensorInfo, type VirtualSensor } from "../models/sensor";
import {
  Box,
  Checkbox,
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

export interface SensorTableProps {
  sensors: SensorInfo[];
  title?: string;
}

export const SensorTable: React.FC<SensorTableProps> = ({ title, sensors }) => {
  const [selectedSensors, setSelectedSensors] = useState<VirtualSensor[]>([]);
  const [dialogTitle, setDialogTitle] = useState<string>("");
  return (
    <Box marginTop={1}>
      {title && (
        <Typography variant="h6" marginBottom={2}>
          {title}
        </Typography>
      )}
      <SensorsDialog
        isOpen={selectedSensors.length > 0}
        onClose={() => {
          setSelectedSensors([]);
          setDialogTitle("");
        }}
        sensors={selectedSensors}
        title={dialogTitle}
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
                      if (r.sensors.length > 0) {
                        setSelectedSensors(r.sensors);
                        setDialogTitle(`Sensors for ${r.name}`);
                      }
                    }}
                    sx={{
                      cursor: r.sensors.length > 0 ? "pointer" : "default",
                      "&:hover":
                        r.sensors.length > 0
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
                  </TableRow>
                );
              })}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};
