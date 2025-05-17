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

export interface MeasurementsDialogProps {
  measurements: Measurement[];
  isOpen: boolean;
  onClose: () => void;
  title?: string;
}

export const MeasurementsDialog: React.FC<MeasurementsDialogProps> = ({
  measurements,
  isOpen,
  onClose,
  title,
}) => {
  const columns: GridColDef[] = [
    {
      field: "sensorValue",
      headerName: "Value",
      type: "number",
      flex: 1,
      valueFormatter: (_value, row) => {
        return formatMeasurement(row, true);
      },
    },
    {
      field: "timestamp",
      headerName: "Timestamp",
      type: "dateTime",
      flex: 1,
      minWidth: 170,
      valueFormatter: (_value, row) => {
        return getFormattedDate((row as Measurement).timestamp, true);
      },
    },
  ];
  return (
    <Dialog open={isOpen} onClose={onClose} maxWidth="xl">
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
        <Box sx={{ overflow: "auto", maxHeight: "600px" }}>
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
            sx={{
              border: 0,
              minWidth: 600,
              maxHeight: "100%",
            }}
            initialState={{
              sorting: {
                sortModel: [{ field: "timestamp", sort: "desc" }],
              },
            }}
          />
        </Box>
      </DialogContent>
    </Dialog>
  );
};
