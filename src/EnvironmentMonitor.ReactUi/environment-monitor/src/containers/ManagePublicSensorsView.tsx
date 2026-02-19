import { useEffect, useState } from "react";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useApiHook } from "../hooks/apiHook";
import { Box, Button, IconButton, Tooltip } from "@mui/material";
import { Add, Refresh } from "@mui/icons-material";
import { useDispatch, useSelector } from "react-redux";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";
import { PublicSensorTable } from "../components/PublicSensors/PublicSensorTable";
import { PublicSensorDialog } from "../components/PublicSensors/ManagePublicSensorsDialog";
import type { Sensor } from "../models/sensor";
import type { AddOrUpdatePublicSensorDto } from "../models/managePublicSensorsRequest";
import { getDevices, getSensors } from "../reducers/measurementReducer";

export const ManagePublicSensorsView: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [publicSensors, setPublicSensors] = useState<Sensor[]>([]);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingSensor, setEditingSensor] = useState<Sensor | undefined>(
    undefined,
  );

  const dispatch = useDispatch();
  const publicSensorHook = useApiHook().publicSensorHook;
  const allSensors = useSelector(getSensors);
  const devices = useSelector(getDevices);

  const loadPublicSensors = () => {
    setIsLoading(true);
    publicSensorHook
      .getPublicSensors()
      .then((res) => {
        setPublicSensors(res);
      })
      .catch((error) => {
        console.error("Failed to load public sensors:", error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  useEffect(() => {
    loadPublicSensors();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSave = (sensor: AddOrUpdatePublicSensorDto) => {
    const isEdit = !!sensor.identifier;
    setIsLoading(true);
    publicSensorHook
      .managePublicSensors({ addOrUpdate: [sensor], remove: [] })
      .then((res) => {
        setPublicSensors(res);
        setDialogOpen(false);
        setEditingSensor(undefined);
        dispatch(
          addNotification({
            title: isEdit
              ? "Public sensor updated successfully"
              : "Public sensor added successfully",
            body: "",
            severity: "success",
          }),
        );
      })
      .catch((error) => {
        console.error("Failed to save public sensor:", error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleEditClick = (sensor: Sensor) => {
    setEditingSensor(sensor);
    setDialogOpen(true);
  };

  const handleDelete = (sensor: Sensor) => {
    dispatch(
      setConfirmDialog({
        onConfirm: () => {
          setIsLoading(true);
          publicSensorHook
            .managePublicSensors({
              addOrUpdate: [],
              remove: [sensor.identifier],
            })
            .then((res) => {
              setPublicSensors(res);
              dispatch(
                addNotification({
                  title: `Public sensor "${sensor.name}" removed`,
                  body: "",
                  severity: "success",
                }),
              );
            })
            .catch((error) => {
              console.error("Failed to remove public sensor:", error);
            })
            .finally(() => {
              setIsLoading(false);
            });
        },
        title: "Remove Public Sensor",
        body: `Are you sure you want to remove the public sensor "${sensor.name}"?`,
      }),
    );
  };

  return (
    <AppContentWrapper
      title="Manage Public Sensors"
      isLoading={isLoading}
      titleComponent={
        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
          <Tooltip title="Refresh">
            <IconButton onClick={loadPublicSensors} size="medium">
              <Refresh />
            </IconButton>
          </Tooltip>
          <Button
            startIcon={<Add />}
            variant="contained"
            size="small"
            onClick={() => {
              setEditingSensor(undefined);
              setDialogOpen(true);
            }}
          >
            Add
          </Button>
        </Box>
      }
    >
      <PublicSensorTable
        sensors={publicSensors}
        allSensors={allSensors}
        onEdit={handleEditClick}
        onDelete={handleDelete}
      />

      <PublicSensorDialog
        open={dialogOpen}
        onClose={() => {
          setDialogOpen(false);
          setEditingSensor(undefined);
        }}
        onSave={handleSave}
        allSensors={allSensors}
        devices={devices}
        editingSensor={editingSensor}
      />
    </AppContentWrapper>
  );
};
