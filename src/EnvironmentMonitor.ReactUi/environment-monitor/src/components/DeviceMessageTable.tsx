import { useEffect, useState } from "react";
import { useApiHook } from "../hooks/apiHook";
import type { GetDeviceMessagesModel } from "../models/getDeviceMessagesModel";
import type { PaginatedResult } from "../models/paginatedResult";
import type { DeviceMessage } from "../models/deviceMessage";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { Box, Checkbox } from "@mui/material";
import moment from "moment";
import { useSelector } from "react-redux";
import { getDevices, getLocations } from "../reducers/measurementReducer";

interface Props {
  model: GetDeviceMessagesModel | undefined;
  onLoadingChange?: (state: boolean) => void;
}

export const DeviceMessagesTable: React.FC<Props> = ({
  model,
  onLoadingChange,
}) => {
  const hook = useApiHook().deviceHook;
  const [getModel, setGetModel] = useState<GetDeviceMessagesModel | undefined>(
    undefined
  );
  const [isLoading, setIsLoading] = useState(false);
  const devices = useSelector(getDevices);
  const locations = useSelector(getLocations);
  const [paginationModel, setPaginationModel] = useState<
    PaginatedResult<DeviceMessage> | undefined
  >(undefined);

  useEffect(() => {
    setGetModel({
      ...model,
      pageNumber: 1,
      pageSize: paginationModel?.pageSize ?? 50,
      from:
        model?.from ??
        moment()
          .local(true)
          .add(-1 * 7, "day")
          .utc(true),
      isDescending: model?.isDescending ?? true,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [model]);

  useEffect(() => {
    if (!getModel) {
      return;
    }
    if (onLoadingChange) {
      onLoadingChange(isLoading);
    }
    setIsLoading(true);
    hook
      .getDeviceMessage(getModel)
      .then((res) => {
        setPaginationModel(res);
      })
      .catch(console.error)
      .finally(() => setIsLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [getModel]);

  const getDeviceLabel = (deviceId: number) => {
    const matchingDevice = devices.find((d) => d.id === deviceId);
    const matchingLocation = locations.find(
      (l) => l.id === matchingDevice?.locationId
    );
    return matchingDevice
      ? `${matchingLocation ? `${matchingLocation.name} - ` : ""}${
          matchingDevice.name
        }`
      : deviceId;
  };
  const columns: GridColDef[] = [
    { field: "timeStamp", headerName: "Timestamp", flex: 1, sortable: false },
    { field: "identifier", headerName: "Identifier", flex: 1, sortable: false },
    {
      field: "deviceId",
      headerName: "Device ",
      flex: 1,
      sortable: false,
      valueGetter: (_value, row) => {
        const deviceMessageRow = row as DeviceMessage;
        return getDeviceLabel(deviceMessageRow.deviceId);
      },
    },
    {
      field: "isDuplicate",
      headerName: "Device ",
      flex: 1,
      sortable: false,
      renderCell: (params) => (
        <Checkbox checked={Boolean(params.value)} disabled />
      ),
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
        rowCount={paginationModel?.totalCount}
        pageSizeOptions={[25, 50, 100]}
        paginationMode="server"
        sortingMode="server"
        paginationModel={{
          page: paginationModel?.pageNumber ?? 1,
          pageSize: paginationModel?.pageSize ?? 50,
        }}
        density="compact"
        onPaginationModelChange={(newModel) => {
          console.log(newModel);
          setGetModel((prev) => ({
            ...prev,
            pageNumber:
              newModel.pageSize !== paginationModel?.pageSize
                ? 1
                : newModel.page,
            pageSize: newModel.pageSize,
            from: model?.from
              ? model.from
              : moment()
                  .local(true)
                  .add(-1 * 7, "day")
                  .utc(true),
            to: model?.to,
            isDescending: true,
          }));
        }}
      />
    </Box>
  );
};
