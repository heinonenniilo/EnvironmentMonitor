import {
  Button,
  Box,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  MenuItem,
  TextField,
  Typography,
} from "@mui/material";
import { Add, Close, Delete, Remove } from "@mui/icons-material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { useMemo, useState, useEffect } from "react";
import { useSelector } from "react-redux";
import type { Sensor, VirtualSensor } from "../../models/sensor";
import {
  getDeviceInfos,
  getDevices,
  getSensors,
} from "../../reducers/measurementReducer";
import {
  getAvailableMeasurementTypes,
  getMeasurementTypeDisplayName,
  getMeasurementUnit,
} from "../../utilities/measurementUtils";
import type { MeasurementTypes } from "../../enums/measurementTypes";
import type { AddVirtualSensorRowDto } from "../../models/updateVirtualSensorRows";
import { getEntityTitle } from "../../utilities/entityUtils";

export interface SensorsDialogProps {
  sensors: VirtualSensor[];
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  location?: string;
  editable?: boolean;
  onSave?: (
    rowsToAdd: AddVirtualSensorRowDto[],
    rowsToDelete: string[],
  ) => Promise<void>;
}

interface EditableVirtualSensorRow {
  rowId: string;
  sensor: Sensor;
  typeId?: number;
  isNew: boolean;
  isRemoved: boolean;
}

export const SensorsDialog: React.FC<SensorsDialogProps> = ({
  sensors,
  isOpen,
  onClose,
  title,
  location,
  editable,
  onSave,
}) => {
  const devices = useSelector(getDevices);
  const deviceInfos = useSelector(getDeviceInfos);
  const allSensors = useSelector(getSensors);
  const [rows, setRows] = useState<EditableVirtualSensorRow[]>([]);
  const [selectedDeviceIdentifier, setSelectedDeviceIdentifier] = useState("");
  const [selectedSensorIdentifier, setSelectedSensorIdentifier] = useState("");
  const [selectedTypeId, setSelectedTypeId] = useState<string>("");
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    setRows(
      sensors.map((sensor) => ({
        rowId: sensor.sensor.identifier,
        sensor: sensor.sensor,
        typeId: sensor.typeId ?? undefined,
        isNew: false,
        isRemoved: false,
      })),
    );
    setSelectedDeviceIdentifier("");
    setSelectedSensorIdentifier("");
    setSelectedTypeId("");
  }, [isOpen, sensors]);

  const deviceInfoMap = useMemo(
    () =>
      new Map(
        deviceInfos.map((deviceInfo) => [
          deviceInfo.device.identifier,
          deviceInfo.isVirtual,
        ]),
      ),
    [deviceInfos],
  );

  const availableSensors = useMemo(() => {
    return allSensors
      .filter((sensor) => {
        if (
          rows.some(
            (row) =>
              row.sensor.identifier === sensor.identifier && !row.isRemoved,
          )
        ) {
          return false;
        }

        const isVirtualDevice = deviceInfoMap.get(sensor.parentIdentifier);
        if (isVirtualDevice === true) {
          return false;
        }

        const parentDevice = devices.find(
          (device) => device.identifier === sensor.parentIdentifier,
        );
        if (location && parentDevice?.locationIdentifier !== location) {
          return false;
        }

        if (selectedDeviceIdentifier) {
          return sensor.parentIdentifier === selectedDeviceIdentifier;
        }

        return true;
      })
      .sort((a, b) => a.name.localeCompare(b.name));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [allSensors, deviceInfoMap, rows, selectedDeviceIdentifier]);

  const deviceOptions = useMemo(() => {
    return [...devices]
      .filter((device) => {
        if (deviceInfoMap.get(device.identifier) === true) {
          return false;
        }

        if (location && device.locationIdentifier !== location) {
          return false;
        }

        return true;
      })
      .sort((a, b) => getEntityTitle(a).localeCompare(getEntityTitle(b)));
  }, [deviceInfoMap, devices, location]);

  const rowsToDelete = useMemo(() => {
    return rows
      .filter((row) => !row.isNew && row.isRemoved)
      .map((row) => row.sensor.identifier);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [rows, sensors]);

  const rowsToAdd = useMemo(() => {
    return rows
      .filter((row) => row.isNew && !row.isRemoved)
      .map((row) => ({
        valueSensorIdentifier: row.sensor.identifier,
        typeId: row.typeId,
      }));
  }, [rows]);

  const isDirty = rowsToAdd.length > 0 || rowsToDelete.length > 0;

  const columns: GridColDef[] = [
    {
      field: "changeIndicator",
      headerName: "",
      width: 48,
      sortable: false,
      filterable: false,
      renderCell: (params) => {
        const row = params.row as EditableVirtualSensorRow;
        if (row.isNew && !row.isRemoved) {
          return <Add fontSize="small" color="success" />;
        }
        if (row.isRemoved) {
          return <Remove fontSize="small" color="error" />;
        }
        return null;
      },
    },
    {
      field: "name",
      headerName: "Name",
      minWidth: 150,
      flex: 2,
      valueGetter: (_value, row) => {
        const sensor = row as EditableVirtualSensorRow;
        if (!sensor) {
          return "";
        }
        return sensor.sensor.displayName ?? sensor.sensor.name;
      },
    },
    {
      field: "deviceIdentifier",
      headerName: "Device",
      minWidth: 130,
      flex: 1,
      valueGetter: (_value, row) => {
        const sensor = row as EditableVirtualSensorRow;
        const matchingDevice = devices.find(
          (d) => d.identifier === sensor.sensor.parentIdentifier,
        );
        return matchingDevice
          ? getEntityTitle(matchingDevice)
          : sensor.sensor.parentIdentifier;
      },
    },
    {
      field: "typeId",
      headerName: "Type",
      minWidth: 130,
      flex: 1,
      valueGetter: (_value, row) => {
        const sensor = row as EditableVirtualSensorRow;
        return getMeasurementUnit(sensor.typeId as MeasurementTypes);
      },
    },
  ];

  if (editable) {
    columns.push({
      field: "actions",
      headerName: "",
      sortable: false,
      filterable: false,
      width: 70,
      renderCell: (params) => {
        const row = params.row as EditableVirtualSensorRow;
        return (
          <IconButton
            size="small"
            color={row.isRemoved ? "primary" : "error"}
            onClick={() => {
              setRows((current) =>
                current.flatMap((item) => {
                  if (item.rowId !== row.rowId) {
                    return [item];
                  }

                  if (item.isNew) {
                    if (item.isRemoved) {
                      return [{ ...item, isRemoved: false }];
                    }

                    return [];
                  }

                  return [{ ...item, isRemoved: !item.isRemoved }];
                }),
              );
            }}
            title={
              row.isRemoved
                ? "Restore sensor"
                : row.isNew
                  ? "Revert added sensor"
                  : "Mark sensor for removal"
            }
          >
            {row.isRemoved ? (
              <Close fontSize="small" />
            ) : (
              <Delete fontSize="small" />
            )}
          </IconButton>
        );
      },
    });
  }

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
            gap: 2,
          }}
        >
          {editable && (
            <Box
              display="flex"
              flexDirection={{ xs: "column", md: "row" }}
              gap={2}
              mt={1}
            >
              <TextField
                select
                label="Device Filter"
                value={selectedDeviceIdentifier}
                onChange={(event) => {
                  setSelectedDeviceIdentifier(event.target.value);
                  setSelectedSensorIdentifier("");
                }}
                fullWidth
                size="small"
              >
                <MenuItem value="">All devices</MenuItem>
                {deviceOptions.map((device) => (
                  <MenuItem key={device.identifier} value={device.identifier}>
                    {getEntityTitle(device)}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                select
                label="Source Sensor"
                value={selectedSensorIdentifier}
                onChange={(event) =>
                  setSelectedSensorIdentifier(event.target.value)
                }
                fullWidth
                size="small"
              >
                <MenuItem value="">Select sensor</MenuItem>
                {availableSensors.map((sensor) => {
                  const device = devices.find(
                    (item) => item.identifier === sensor.parentIdentifier,
                  );

                  return (
                    <MenuItem key={sensor.identifier} value={sensor.identifier}>
                      {sensor.name}
                      {device ? ` - ${getEntityTitle(device)}` : ""}
                    </MenuItem>
                  );
                })}
              </TextField>
              <TextField
                select
                label="Measurement Type"
                value={selectedTypeId}
                onChange={(event) => setSelectedTypeId(event.target.value)}
                fullWidth
                size="small"
              >
                <MenuItem value="">No filter</MenuItem>
                {getAvailableMeasurementTypes().map((value) => (
                  <MenuItem key={value} value={value}>
                    {getMeasurementTypeDisplayName(
                      value as MeasurementTypes,
                      true,
                    )}
                  </MenuItem>
                ))}
              </TextField>
              <Button
                variant="contained"
                onClick={() => {
                  const selectedSensor = availableSensors.find(
                    (sensor) => sensor.identifier === selectedSensorIdentifier,
                  );

                  if (!selectedSensor) {
                    return;
                  }

                  setRows((current) => [
                    ...current,
                    {
                      rowId: `new-${selectedSensor.identifier}`,
                      sensor: selectedSensor,
                      typeId: selectedTypeId
                        ? Number(selectedTypeId)
                        : undefined,
                      isNew: true,
                      isRemoved: false,
                    },
                  ]);
                  setSelectedSensorIdentifier("");
                }}
                disabled={!selectedSensorIdentifier}
              >
                Add
              </Button>
            </Box>
          )}
          {editable && (
            <Typography variant="body2" color="text.secondary">
              Added rows are marked with `+`. Removed rows stay visible, are
              struck through, and can be restored before saving.
            </Typography>
          )}
          <DataGrid
            rows={rows}
            columns={columns}
            density="compact"
            getRowId={(row) => {
              if (row) {
                return (row as EditableVirtualSensorRow).rowId;
              }
              return "";
            }}
            initialState={{
              sorting: {
                sortModel: [{ field: "name", sort: "asc" }],
              },
            }}
            disableRowSelectionOnClick
            getRowClassName={(params) => {
              const row = params.row as EditableVirtualSensorRow;
              if (row.isNew && !row.isRemoved) {
                return "virtual-sensor-row-added";
              }
              if (row.isRemoved) {
                return "virtual-sensor-row-removed";
              }
              return "";
            }}
            sx={{
              "& .virtual-sensor-row-added": {
                backgroundColor: "success.light",
                "& .MuiDataGrid-cell": {
                  borderColor: "success.dark",
                },
              },
              "& .virtual-sensor-row-removed": {
                backgroundColor: "action.hover",
                textDecoration: "line-through",
              },
            }}
          />
        </Box>
      </DialogContent>
      {editable && (
        <DialogActions>
          <Button onClick={onClose} color="inherit">
            Cancel
          </Button>
          <Button
            onClick={async () => {
              if (!onSave) {
                return;
              }

              setIsSaving(true);
              try {
                await onSave(rowsToAdd, rowsToDelete);
                setRows((current) =>
                  current
                    .filter((row) => !row.isRemoved)
                    .map((row) => ({
                      ...row,
                      rowId: row.sensor.identifier,
                      isNew: false,
                      isRemoved: false,
                    })),
                );
              } finally {
                setIsSaving(false);
              }
            }}
            variant="contained"
            disabled={!isDirty || isSaving}
          >
            Save
          </Button>
        </DialogActions>
      )}
    </Dialog>
  );
};
