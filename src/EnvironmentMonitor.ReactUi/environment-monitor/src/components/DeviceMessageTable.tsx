import { useEffect, useState } from "react";
import { useApiHook } from "../hooks/apiHook";
import type { GetDeviceMessagesModel } from "../models/getDeviceMessagesModel";
import type { PaginatedResult } from "../models/paginatedResult";
import type { DeviceMessage } from "../models/deviceMessage";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { Box, Checkbox, useMediaQuery, useTheme } from "@mui/material";
import { useSelector } from "react-redux";
import { getLocations } from "../reducers/measurementReducer";
import { getFormattedDate } from "../utilities/datetimeUtils";
import { defaultStart } from "../containers/DeviceMessagesView";
import type { DeviceInfo } from "../models/deviceInfo";
import { Link } from "react-router";
import { getCommunicationChannelDisplayName } from "../enums/communicationChannels";

interface Props {
  model: GetDeviceMessagesModel | undefined;
  onLoadingChange?: (state: boolean) => void;
  onRowClick?: (message: DeviceMessage) => void;
  renderLinkToDevice?: boolean;
  deviceInfos: DeviceInfo[];
}

export const DeviceMessagesTable: React.FC<Props> = ({
  model,
  onLoadingChange,
  onRowClick,
  renderLinkToDevice,
  deviceInfos,
}) => {
  const hook = useApiHook().deviceHook;
  const [getModel, setGetModel] = useState<GetDeviceMessagesModel | undefined>(
    undefined,
  );
  const [isLoading, setIsLoading] = useState(false);

  const locations = useSelector(getLocations);
  const [paginationModel, setPaginationModel] = useState<
    PaginatedResult<DeviceMessage> | undefined
  >(undefined);
  const theme = useTheme();
  const drawDesktop = useMediaQuery(theme.breakpoints.up("lg"));

  useEffect(() => {
    if (!model) {
      return;
    }
    setGetModel({
      ...model,
      pageNumber: 0,
      pageSize: paginationModel?.pageSize ?? model.pageSize,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [model]);

  useEffect(() => {
    if (!getModel) {
      return;
    }
    if (onLoadingChange) {
      onLoadingChange(true);
    }
    setIsLoading(true);
    hook
      .getDeviceMessage(getModel)
      .then((res) => {
        const duplicateCounters = new Map<string, number>();
        const processedItems = res.items.map((item) => {
          if (!item.isDuplicate) {
            return { ...item, uniqueRowId: item.identifier };
          } else {
            const currentCount = duplicateCounters.get(item.identifier) || 0;
            const newCount = currentCount + 1;
            duplicateCounters.set(item.identifier, newCount);
            return {
              ...item,
              uniqueRowId: `${item.identifier}-dup-${newCount}`,
            };
          }
        });

        setPaginationModel({
          ...res,
          items: processedItems,
        });
      })
      .catch(console.error)
      .finally(() => {
        setIsLoading(false);
        if (onLoadingChange) {
          onLoadingChange(false);
        }
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [getModel]);

  const getDeviceLabel = (deviceIdentifier: string) => {
    const matchingDevice = deviceInfos.find(
      (d) => d.device.identifier === deviceIdentifier,
    );
    const matchingLocation = locations.find(
      (l) => l.identifier === matchingDevice?.device.locationIdentifier,
    );
    return matchingDevice
      ? `${matchingLocation ? `${matchingLocation.name} - ` : ""}${
          matchingDevice.device.name
        }`
      : deviceIdentifier;
  };
  const columns: GridColDef[] = [
    {
      field: "timeStamp",
      headerName: "Timestamp",
      flex: 1,
      sortable: false,
      minWidth: 150,
      valueFormatter: (value) => {
        if (!value) {
          return "";
        }
        return getFormattedDate(value as Date, true, true);
      },
    },
    {
      field: "identifier",
      headerName: "Identifier",
      minWidth: 200,
      flex: 1,
      sortable: false,
    },
    {
      field: "deviceId",
      headerName: "Device ",
      flex: 1,
      sortable: false,
      minWidth: 150,
      valueGetter: (_value, row) => {
        const deviceMessageRow = row as DeviceMessage;
        return getDeviceLabel(deviceMessageRow.deviceIdentifier);
      },
      renderCell: renderLinkToDevice
        ? (params) => {
            return (
              <Link
                to={`/devices/${
                  (params.row as DeviceMessage).deviceIdentifier
                }`}
              >
                {getDeviceLabel((params.row as DeviceMessage).deviceIdentifier)}
              </Link>
            );
          }
        : undefined,
    },
    {
      field: "isDuplicate",
      headerName: "Duplicate ",
      flex: 1,
      sortable: false,
      minWidth: 80,
      renderCell: (params) => (
        <Checkbox checked={Boolean(params.value)} disabled />
      ),
    },
    {
      field: "firstMessage",
      headerName: "First message",
      flex: 1,
      sortable: false,
      minWidth: 80,
      renderCell: (params) => (
        <Checkbox checked={Boolean(params.value)} disabled />
      ),
    },
    {
      field: "sourceId",
      headerName: "Source",
      flex: 1,
      sortable: false,
      minWidth: 120,
      valueGetter: (_value, row) => {
        const deviceMessageRow = row as DeviceMessage;
        return getCommunicationChannelDisplayName(deviceMessageRow.sourceId);
      },
    },
  ];

  return (
    <Box
      sx={{
        flex: 1,
        display: "flex",
        flexDirection: "column",
        minHeight: 0,
        maxHeight: "calc(100vh - 150px)",
      }}
    >
      <DataGrid
        rows={paginationModel?.items ?? []}
        columns={columns}
        disableRowSelectionOnClick
        loading={isLoading}
        pagination
        rowCount={paginationModel?.totalCount ?? 0}
        pageSizeOptions={[25, 50, 100]}
        paginationMode="server"
        sortingMode="server"
        paginationModel={{
          page: paginationModel ? paginationModel.pageNumber : 0,
          pageSize: paginationModel ? paginationModel.pageSize : 50,
        }}
        getRowHeight={drawDesktop ? undefined : () => "auto"}
        density="compact"
        onRowClick={
          onRowClick
            ? (row) => {
                const data = row.row as DeviceMessage;
                if (!data.isDuplicate) {
                  onRowClick(data);
                }
              }
            : undefined
        }
        onPaginationModelChange={(newModel) => {
          console.log(newModel);
          setGetModel((prev) => ({
            ...prev,
            pageNumber:
              newModel.pageSize !== paginationModel?.pageSize
                ? 0
                : newModel.page,
            pageSize: newModel.pageSize,
            from: model?.from ? model.from : defaultStart,
            to: model?.to,
            isDescending: model?.isDescending ?? true,
          }));
        }}
        getRowId={(row) =>
          (row as DeviceMessage).identifier ??
          `${(row as DeviceMessage).identifier}_${(row as DeviceMessage).timeStamp}`
        }
        getRowClassName={(params) =>
          !(params.row as DeviceMessage).isDuplicate && onRowClick
            ? "clickable-row"
            : ""
        }
      />
    </Box>
  );
};
