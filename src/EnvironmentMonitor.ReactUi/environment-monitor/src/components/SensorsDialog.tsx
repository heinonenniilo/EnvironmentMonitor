import {
  Box,
  Dialog,
  DialogContent,
  DialogTitle,
  IconButton,
} from "@mui/material";
import type { Sensor, SensorInfo } from "../models/sensor";
import { Close } from "@mui/icons-material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useSelector } from "react-redux";
import { getDeviceInfos } from "../reducers/measurementReducer";

export interface SensorsDialogProps {
  sensors: Sensor[];
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
    },
    {
      field: "deviceIdentifier",
      headerName: "Device",
      minWidth: 130,
      flex: 1,
      valueGetter: (_value, row) => {
        const sensor = row as Sensor;
        const matchingDevice = devices.find(
          (d) => d.device.identifier === sensor.deviceIdentifier
        );
        return matchingDevice
          ? matchingDevice.device.name
          : sensor.deviceIdentifier;
      },
    },
    {
      field: "scaleMin",
      headerName: "Scale Min",
      type: "number",
      minWidth: 100,
      flex: 1,
      valueFormatter: (value) => {
        return value ?? "-";
      },
    },
    {
      field: "scaleMax",
      headerName: "Scale Max",
      type: "number",
      minWidth: 100,
      flex: 1,
      valueFormatter: (value) => {
        return value ?? "-";
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
                return (row as SensorInfo).identifier;
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
