import {
  Box,
  Dialog,
  DialogContent,
  DialogTitle,
  IconButton,
} from "@mui/material";
import type { Measurement } from "../models/measurement";
import { Close } from "@mui/icons-material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { getFormattedDate } from "../utilities/datetimeUtils";
import { formatMeasurement } from "../utilities/measurementUtils";
import type { Sensor } from "../models/sensor";

export interface MeasurementsDialogProps {
  measurements: Measurement[];
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  sensors?: Sensor[];
}

export const MeasurementsDialog: React.FC<MeasurementsDialogProps> = ({
  measurements,
  isOpen,
  onClose,
  title,
  sensors,
}) => {
  const columns: GridColDef[] = [
    {
      field: "sensorId",
      headerName: "Sensor",
      minWidth: 130,
      flex: 1,
      valueFormatter: (value, row) => {
        const matchingSensor = sensors?.find(
          (s) => s.identifier === (row as Measurement).sensorIdentifier
        );

        if (matchingSensor) {
          return matchingSensor.name;
        }
        return value;
      },
    },
    {
      field: "timestamp",
      headerName: "Timestamp",
      type: "dateTime",
      minWidth: 150,
      align: "left",
      flex: 1,
      headerAlign: "left",
      valueFormatter: (_value, row) => {
        return getFormattedDate((row as Measurement).timestamp, true);
      },
    },
    {
      field: "sensorValue",
      headerName: "Value",
      type: "number",
      minWidth: 120,
      align: "left",
      flex: 1,
      headerAlign: "left",
      valueFormatter: (_value, row) => {
        return formatMeasurement(row, true);
      },
    },
  ];
  return (
    <Dialog open={isOpen} onClose={onClose}>
      <DialogTitle
        sx={{
          display: "flex",
          flexDirection: "row",
          justifyContent: "space-between",
        }}
      >
        <Box>{title ?? ""}</Box>
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
            rows={measurements.map((m, idx) => {
              return { ...m, id: idx };
            })}
            columns={columns}
            density="compact"
            getRowId={(row) => {
              if (row) {
                return (row as Measurement).id ?? 0;
              }
              return "";
            }}
            initialState={{
              sorting: {
                sortModel: [{ field: "timestamp", sort: "desc" }],
              },
            }}
            columnVisibilityModel={!sensors ? { sensorId: false } : undefined}
          />
        </Box>
      </DialogContent>
    </Dialog>
  );
};
