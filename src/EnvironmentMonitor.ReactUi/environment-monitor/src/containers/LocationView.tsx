import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router";
import { Box, IconButton, Typography } from "@mui/material";
import { Add, Delete, DriveFileMove, Edit } from "@mui/icons-material";
import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useApiHook } from "../hooks/apiHook";
import type { LocationModel } from "../models/location";
import type { Device } from "../models/device";
import type { DeviceInfo } from "../models/deviceInfo";
import type { Sensor } from "../models/sensor";
import {
  getDevices,
  getLocations,
  getSensors,
  setDevices,
  setLocations,
} from "../reducers/measurementReducer";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";
import { getEntityTitle } from "../utilities/entityUtils";
import { Collapsible } from "../framework/CollabsibleComponent";
import { LocationSensorsTable } from "../components/Locations/LocationSensorsTable";
import { LocationSensorDialog } from "../components/Locations/LocationSensorDialog";
import { MoveDevicesDialog } from "../components/Locations/MoveDevicesDialog";
import { EditLocationDialog } from "../components/Locations/EditLocationDialog";
import { LocationInfo } from "../components/Locations/LocationInfo";
import { DeviceTable } from "../components/Devices/DeviceTable";
import { routes } from "../utilities/routes";

export const LocationView: React.FC = () => {
  const { locationId } = useParams<{ locationId?: string }>();
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const locationHook = useApiHook().locationHook;
  const deviceHook = useApiHook().deviceHook;
  const measureHook = useApiHook().measureHook;

  const [location, setLocation] = useState<LocationModel | undefined>(
    undefined,
  );
  const [isLoading, setIsLoading] = useState(false);
  const [sensorDialogOpen, setSensorDialogOpen] = useState(false);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [moveDevicesOpen, setMoveDevicesOpen] = useState(false);
  const [selectedSensor, setSelectedSensor] = useState<Sensor | null>(null);
  const [allDevices, setAllDevices] = useState<Device[]>([]);
  const [locationDeviceInfos, setLocationDeviceInfos] = useState<DeviceInfo[]>(
    [],
  );

  const sensors = useSelector(getSensors);
  const devices = useSelector(getDevices);
  const locations = useSelector(getLocations);

  const loadLocation = () => {
    if (!locationId) {
      return;
    }

    setIsLoading(true);
    locationHook
      .getLocation(locationId)
      .then((response) => {
        setLocation(response);
      })
      .catch((error) => {
        console.error(error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const loadLocationDeviceInfos = () => {
    if (!locationId) {
      setLocationDeviceInfos([]);
      return;
    }

    deviceHook
      .getDeviceInfos([locationId])
      .then((response) => {
        setLocationDeviceInfos(response ?? []);
      })
      .catch((error) => {
        console.error(error);
      });
  };

  const refreshLocations = () => {
    locationHook
      .getLocations({ getDevices: true })
      .then((response) => {
        dispatch(setLocations(response));
      })
      .catch((error) => {
        console.error(error);
      });
  };

  const refreshDevices = () => {
    measureHook
      .getDevices()
      .then((response) => {
        const nextDevices = response ?? [];
        setAllDevices(nextDevices);
        dispatch(setDevices(nextDevices));
      })
      .catch((error) => {
        console.error(error);
      });
  };

  useEffect(() => {
    loadLocation();
    loadLocationDeviceInfos();
    refreshDevices();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [locationId]);

  useEffect(() => {
    if (devices.length > 0) {
      setAllDevices(devices);
    }
  }, [devices]);

  const availableSensors = () => {
    const locationDeviceIdentifiers = new Set(
      (location?.devices ?? []).map((device) => device.identifier),
    );
    const assignedLocationSensorIdentifiers = new Set(
      locations.flatMap((item) =>
        item.locationSensors.map((locationSensor) => locationSensor.identifier),
      ),
    );

    return sensors.filter((sensor) => {
      const belongsToLocationDevice = locationDeviceIdentifiers.has(
        sensor.parentIdentifier,
      );
      const alreadyAdded = assignedLocationSensorIdentifiers.has(
        sensor.identifier,
      );

      return belongsToLocationDevice && !alreadyAdded;
    });
  };

  const movableDevices = useMemo(
    () =>
      allDevices
        .filter((device) => device.locationIdentifier !== location?.identifier)
        .sort((a, b) => getEntityTitle(a).localeCompare(getEntityTitle(b))),
    [allDevices, location?.identifier],
  );

  const handleLocationUpdate = (
    updatedLocation: LocationModel,
    message: string,
  ) => {
    setLocation(updatedLocation);
    refreshLocations();
    dispatch(
      addNotification({
        title: message,
        body: "",
        severity: "success",
      }),
    );
  };

  const handleSaveSensor = (model: {
    locationIdentifier: string;
    sensorIdentifier: string;
    name: string;
    typeId?: number;
  }) => {
    setIsLoading(true);
    const action = selectedSensor
      ? locationHook.updateLocationSensor(model)
      : locationHook.addLocationSensor(model);

    action
      .then((response) => {
        handleLocationUpdate(
          response,
          selectedSensor
            ? "Location sensor updated successfully"
            : "Location sensor added successfully",
        );
        setSensorDialogOpen(false);
        setSelectedSensor(null);
      })
      .catch((error) => {
        console.error(error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleDeleteSensor = (sensor: Sensor) => {
    if (!location) {
      return;
    }

    dispatch(
      setConfirmDialog({
        title: "Delete location sensor?",
        body: `Are you sure you want to delete "${sensor.name}" from ${location.name}?`,
        onConfirm: () => {
          setIsLoading(true);
          locationHook
            .deleteLocationSensor(location.identifier, sensor.identifier)
            .then((response) => {
              handleLocationUpdate(
                response,
                "Location sensor deleted successfully",
              );
            })
            .catch((error) => {
              console.error(error);
            })
            .finally(() => {
              setIsLoading(false);
            });
        },
      }),
    );
  };

  const handleMoveDevices = (deviceIdentifiers: string[]) => {
    if (!location) {
      return;
    }

    setIsLoading(true);
    locationHook
      .moveDevicesToLocation({
        locationIdentifier: location.identifier,
        deviceIdentifiers,
      })
      .then(() => {
        dispatch(
          addNotification({
            title: "Devices moved successfully",
            body: "",
            severity: "success",
          }),
        );
        refreshDevices();
        refreshLocations();
        loadLocation();
        loadLocationDeviceInfos();
        setMoveDevicesOpen(false);
      })
      .catch((error) => {
        console.error(error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleDeleteLocation = () => {
    if (!location) {
      return;
    }

    dispatch(
      setConfirmDialog({
        title: "Delete location?",
        body: `Are you sure you want to delete "${location.name}"?`,
        onConfirm: () => {
          setIsLoading(true);
          locationHook
            .deleteLocation(location.identifier)
            .then(() => {
              refreshLocations();
              dispatch(
                addNotification({
                  title: "Location deleted successfully",
                  body: "",
                  severity: "success",
                }),
              );
              navigate(routes.locations);
            })
            .catch((error) => {
              console.error(error);
            })
            .finally(() => {
              setIsLoading(false);
            });
        },
      }),
    );
  };

  const handleEditLocation = (model: LocationModel) => {
    setIsLoading(true);
    locationHook
      .updateLocation(model)
      .then((response) => {
        handleLocationUpdate(response, "Location updated successfully");
        setEditDialogOpen(false);
      })
      .catch((error) => {
        console.error(error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  return (
    <AppContentWrapper
      title={getEntityTitle(location)}
      isLoading={isLoading}
      titleComponent={
        location ? (
          <Box display="flex" alignItems="center" gap={1}>
            <IconButton
              title="Edit location"
              onClick={() => setEditDialogOpen(true)}
            >
              <Edit />
            </IconButton>
            <IconButton
              color="error"
              title="Delete location"
              onClick={handleDeleteLocation}
            >
              <Delete />
            </IconButton>
          </Box>
        ) : undefined
      }
    >
      {location ? (
        <Box display="flex" flexDirection="column" gap={2}>
          <Collapsible title="Info" isOpen={true}>
            <LocationInfo location={location} />
          </Collapsible>

          <Collapsible
            title="Location Sensors"
            isOpen={true}
            customComponent={
              <IconButton
                size="small"
                title="Add location sensor"
                onClick={() => {
                  setSelectedSensor(null);
                  setSensorDialogOpen(true);
                }}
              >
                <Add />
              </IconButton>
            }
          >
            <LocationSensorsTable
              sensors={location.locationSensors}
              onEdit={(sensor) => {
                setSelectedSensor(sensor);
                setSensorDialogOpen(true);
              }}
              onDelete={handleDeleteSensor}
            />
          </Collapsible>

          <Collapsible
            title="Devices"
            isOpen={true}
            customComponent={
              <IconButton
                size="small"
                title="Move devices to location"
                onClick={() => setMoveDevicesOpen(true)}
              >
                <DriveFileMove />
              </IconButton>
            }
          >
            <DeviceTable
              devices={locationDeviceInfos}
              renderLink
              renderLinkToDeviceMessages
              hideId
              showDeviceIdentifier
            />
          </Collapsible>
        </Box>
      ) : (
        <Typography variant="body1" p={1}>
          Location not found.
        </Typography>
      )}

      <LocationSensorDialog
        open={sensorDialogOpen}
        locationIdentifier={location?.identifier ?? ""}
        sensor={selectedSensor}
        availableSensors={availableSensors()}
        allSensors={sensors}
        devices={location?.devices ?? []}
        onClose={() => {
          setSensorDialogOpen(false);
          setSelectedSensor(null);
        }}
        onSave={handleSaveSensor}
      />
      <MoveDevicesDialog
        open={moveDevicesOpen}
        locationName={getEntityTitle(location)}
        devices={movableDevices}
        onClose={() => setMoveDevicesOpen(false)}
        onSave={handleMoveDevices}
      />
      <EditLocationDialog
        open={editDialogOpen}
        location={location}
        onClose={() => setEditDialogOpen(false)}
        onSave={handleEditLocation}
      />
    </AppContentWrapper>
  );
};
