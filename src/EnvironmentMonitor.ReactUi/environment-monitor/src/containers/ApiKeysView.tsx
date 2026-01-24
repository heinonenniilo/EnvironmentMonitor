import React, { useEffect, useState } from "react";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useApiHook } from "../hooks/apiHook";
import { Box, IconButton, Tooltip } from "@mui/material";
import { Add, Refresh } from "@mui/icons-material";
import { ApiKeyTable } from "../components/ApiKeys/ApiKeyTable";
import { ApiKeyDetailsDialog } from "../components/ApiKeys/ApiKeyDetailsDialog";
import { CreateApiKeyDialog } from "../components/ApiKeys/CreateApiKeyDialog";
import { ApiKeyDialog } from "../components/ApiKeyDialog";
import type { ApiKeyDto } from "../models/apiKey";
import type { DeviceInfo } from "../models/deviceInfo";
import type { LocationModel } from "../models/location";
import { useDispatch } from "react-redux";
import {
  addNotification,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";

export const ApiKeysView: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [apiKeys, setApiKeys] = useState<ApiKeyDto[]>([]);
  const [selectedApiKey, setSelectedApiKey] = useState<ApiKeyDto | null>(null);
  const [detailsDialogOpen, setDetailsDialogOpen] = useState(false);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [generatedKeyDialogOpen, setGeneratedKeyDialogOpen] = useState(false);
  const [generatedApiKey, setGeneratedApiKey] = useState<{
    apiKey: string;
    id: string;
    description?: string;
    created: string;
  } | null>(null);
  const [devices, setDevices] = useState<DeviceInfo[]>([]);
  const [locations, setLocations] = useState<LocationModel[]>([]);

  const dispatch = useDispatch();
  const apiKeysHook = useApiHook().apiKeysHook;
  const deviceHook = useApiHook().deviceHook;
  const locationHook = useApiHook().locationHook;

  const loadApiKeys = () => {
    setIsLoading(true);
    apiKeysHook
      .getAllApiKeys()
      .then((res) => {
        setApiKeys(res);
      })
      .catch((error) => {
        console.error("Failed to load API keys:", error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const loadDevices = () => {
    deviceHook.getDeviceInfos().then((res) => {
      if (res) {
        setDevices(res);
      }
    });
  };

  const loadLocations = () => {
    locationHook.getLocations().then((res) => {
      setLocations(res);
    });
  };

  useEffect(() => {
    loadApiKeys();
    loadDevices();
    loadLocations();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleViewDetails = (apiKey: ApiKeyDto) => {
    setSelectedApiKey(apiKey);
    setDetailsDialogOpen(true);
  };

  const handleDelete = (apiKey: ApiKeyDto) => {
    dispatch(
      setConfirmDialog({
        onConfirm: () => {
          performDelete(apiKey.id);
        },
        title: "Delete API Key",
        body: `Are you sure you want to delete the API key "${apiKey.description || apiKey.id}"? This action cannot be undone.`,
      }),
    );
  };

  const performDelete = (id: string) => {
    setIsLoading(true);
    apiKeysHook
      .deleteApiKey(id)
      .then((success) => {
        if (success) {
          dispatch(
            addNotification({
              title: "API key deleted successfully",
              body: "",
              severity: "success",
            }),
          );
          loadApiKeys();
        }
      })
      .catch((error) => {
        console.error("Failed to delete API key:", error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleToggleEnabled = (apiKey: ApiKeyDto) => {
    setIsLoading(true);
    apiKeysHook
      .updateApiKey(apiKey.id, { enabled: !apiKey.enabled })
      .then(() => {
        dispatch(
          addNotification({
            title: `API key ${!apiKey.enabled ? "enabled" : "disabled"} successfully`,
            body: "",
            severity: "success",
          }),
        );
        loadApiKeys();
      })
      .catch((error) => {
        console.error("Failed to update API key status:", error);
        dispatch(
          addNotification({
            title: "Failed to update API key status",
            body: "An error occurred while updating the API key status",
            severity: "error",
          }),
        );
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleCreateApiKey = (
    description: string,
    deviceIds: string[],
    locationIds: string[],
  ) => {
    setIsLoading(true);
    apiKeysHook
      .createApiKey({
        deviceIds,
        locationIds,
        description,
      })
      .then((response) => {
        setGeneratedApiKey(response);
        setCreateDialogOpen(false);
        setGeneratedKeyDialogOpen(true);
        dispatch(
          addNotification({
            title: "API Key generated successfully",
            body: "Make sure to save the API key secret now. You won't be able to see it again.",
            severity: "success",
          }),
        );
        // Reload the list
        loadApiKeys();
      })
      .catch((error) => {
        console.error("Failed to create API key:", error);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleCloseGeneratedKeyDialog = () => {
    setGeneratedKeyDialogOpen(false);
    setGeneratedApiKey(null);
  };

  return (
    <AppContentWrapper
      title="API Keys"
      isLoading={isLoading}
      titleComponent={
        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
          <Tooltip title="Refresh">
            <IconButton onClick={loadApiKeys} size="medium">
              <Refresh />
            </IconButton>
          </Tooltip>
          <Tooltip title="Create New API Key">
            <IconButton
              onClick={() => setCreateDialogOpen(true)}
              size="medium"
              color="primary"
            >
              <Add />
            </IconButton>
          </Tooltip>
        </Box>
      }
    >
      <Box sx={{ p: 2 }}>
        <ApiKeyTable
          apiKeys={apiKeys}
          onViewDetails={handleViewDetails}
          onDelete={handleDelete}
          onToggleEnabled={handleToggleEnabled}
          isLoading={isLoading}
        />
      </Box>

      <ApiKeyDetailsDialog
        open={detailsDialogOpen}
        apiKey={selectedApiKey}
        onClose={() => setDetailsDialogOpen(false)}
        locations={locations}
        devices={devices.map((d) => d.device)}
      />

      <CreateApiKeyDialog
        open={createDialogOpen}
        onClose={() => setCreateDialogOpen(false)}
        onCreate={handleCreateApiKey}
        devices={devices}
        locations={locations}
        isLoading={isLoading}
      />

      <ApiKeyDialog
        open={generatedKeyDialogOpen}
        onClose={handleCloseGeneratedKeyDialog}
        onGenerate={() => {}}
        generatedKey={generatedApiKey}
        isLoading={false}
      />
    </AppContentWrapper>
  );
};
