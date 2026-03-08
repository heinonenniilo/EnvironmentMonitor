import { useEffect, useState } from "react";
import { IconButton } from "@mui/material";
import { Add } from "@mui/icons-material";
import { useDispatch } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useApiHook } from "../hooks/apiHook";
import { LocationTable } from "../components/Locations/LocationTable";
import { LocationDialog } from "../components/Locations/LocationDialog";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";
import { setLocations } from "../reducers/measurementReducer";
import type { LocationModel } from "../models/location";

export const LocationsView: React.FC = () => {
  const dispatch = useDispatch();
  const locationHook = useApiHook().locationHook;
  const [isLoading, setIsLoading] = useState(false);
  const [locations, setLocationsState] = useState<LocationModel[]>([]);
  const [dialogOpen, setDialogOpen] = useState(false);

  const loadLocations = () => {
    setIsLoading(true);
    locationHook
      .getLocations(true)
      .then((response) => {
        setLocationsState(response);
        dispatch(
          setLocations(
            response.map((location) => ({
              ...location,
              devices: undefined,
            })),
          ),
        );
      })
      .catch((error) => {
        console.error(error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  useEffect(() => {
    loadLocations();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleCreateLocation = (name: string) => {
    setIsLoading(true);
    locationHook
      .addLocation({ name })
      .then(() => {
        dispatch(
          addNotification({
            title: "Location added successfully",
            body: "",
            severity: "success",
          }),
        );
        setDialogOpen(false);
        loadLocations();
      })
      .catch((error) => {
        console.error(error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleDeleteLocation = (location: LocationModel) => {
    dispatch(
      setConfirmDialog({
        title: "Delete location?",
        body: `Are you sure you want to delete "${location.name}"?`,
        onConfirm: () => {
          setIsLoading(true);
          locationHook
            .deleteLocation(location.identifier)
            .then(() => {
              dispatch(
                addNotification({
                  title: "Location deleted successfully",
                  body: "",
                  severity: "success",
                }),
              );
              loadLocations();
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

  return (
    <AppContentWrapper
      title="Locations"
      isLoading={isLoading}
      titleComponent={
        <IconButton
          size="small"
          title="Add location"
          onClick={() => setDialogOpen(true)}
        >
          <Add />
        </IconButton>
      }
    >
      <LocationTable
        locations={locations}
        renderLink
        onDelete={handleDeleteLocation}
      />
      <LocationDialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        onSave={handleCreateLocation}
      />
    </AppContentWrapper>
  );
};
