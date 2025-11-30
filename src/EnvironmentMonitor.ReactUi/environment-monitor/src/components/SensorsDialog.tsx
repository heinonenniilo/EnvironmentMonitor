import {
  Box,
  Dialog,
  DialogContent,
  DialogTitle,
  IconButton,
} from "@mui/material";
import type { VirtualSensor } from "../models/sensor";
import { Close } from "@mui/icons-material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useSelector } from "react-redux";
import { getDeviceInfos } from "../reducers/measurementReducer";
import { getMeasurementUnit } from "../utilities/measurementUtils";
import type { MeasurementTypes } from "../enums/measurementTypes";

export interface SensorsDialogProps {
  sensors: VirtualSensor[];
  isOpen: boolean;
  onClose: () => void;
  title?: string;
}

export const SensorsDialog: React.FC<SensorsDialogProps> = ({
  sensors,
  isOpen,
  onClose,
  title,
}) => {
  const devices = useSelector(getDeviceInfos);
  const columns: GridColDef[] = [
    {
      field: "name",
      headerName: "Name",
      minWidth: 150,
      flex: 2,
      valueGetter: (_value, row) => {
        const sensor = row as VirtualSensor;
        if (!sensor) {
          return "";
        }
        return sensor.sensor.name;
      },
    },
    {
      field: "deviceIdentifier",
      headerName: "Device",
      minWidth: 130,
      flex: 1,
      valueGetter: (_value, row) => {
        const sensor = row as VirtualSensor;
        const matchingDevice = devices.find(
          (d) => d.device.identifier === sensor.sensor.parentIdentifier
        );
        return matchingDevice
          ? matchingDevice.device.name
          : sensor.sensor.parentIdentifier;
      },
    },
    {
      field: "typeId",
      headerName: "Type",
      minWidth: 130,
      flex: 1,
      valueGetter: (_value, row) => {
        const sensor = row as VirtualSensor;
        return getMeasurementUnit(sensor.typeId as MeasurementTypes);
      },
    },
  ];

  return (
    <Dialog open={isOpen} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle
        sx={{
          display: "flex",
          flexDirection: "row",
          justifyContent: "space-between",
        }}
      >
        <Box>{title ?? "Sensors"}</Box>
        <Box sx={{ display: "flex", flexBasis: "row" }}>
          <IconButton
            aria-label="close"
            onClick={() => {
              onClose();
            }}
            sx={{
              color: (theme) => theme.palette.grey[500],
            }}
            size="small"
          >
            <Close />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent>
        <Box
          sx={{
            overflow: "auto",
            maxHeight: "calc(100vh - 200px)",
            width: "100%",
            display: "flex",
            flexDirection: "column",
          }}
        >
          <DataGrid
            rows={sensors}
            columns={columns}
            density="compact"
            getRowId={(row) => {
              if (row) {
                return (row as VirtualSensor).sensor.identifier;
              }
              return "";
            }}
            initialState={{
              sorting: {
                sortModel: [{ field: "sensorId", sort: "asc" }],
              },
            }}
          />
        </Box>
      </DialogContent>
    </Dialog>
  );
};
