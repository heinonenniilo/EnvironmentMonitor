import React from "react";
import {
  DataGrid,
  type GridColDef,
  type GridRenderCellParams,
  type GridRowParams,
} from "@mui/x-data-grid";
import { IconButton, Chip, Box, Tooltip } from "@mui/material";
import { Delete, Info } from "@mui/icons-material";
import type { ApiKeyDto } from "../../models/apiKey";
import moment from "moment";

interface ApiKeyTableProps {
  apiKeys: ApiKeyDto[];
  onViewDetails: (apiKey: ApiKeyDto) => void;
  onDelete: (apiKey: ApiKeyDto) => void;
  isLoading?: boolean;
}

export const ApiKeyTable: React.FC<ApiKeyTableProps> = ({
  apiKeys,
  onViewDetails,
  onDelete,
  isLoading,
}) => {
  const getDeviceIds = (apiKey: ApiKeyDto): string[] => {
    return apiKey.claims
      .filter((claim) => claim.type === "DeviceId")
      .map((claim) => claim.value);
  };

  const getLocationIds = (apiKey: ApiKeyDto): string[] => {
    return apiKey.claims
      .filter((claim) => claim.type === "LocationId")
      .map((claim) => claim.value);
  };

  const columns: GridColDef[] = [
    {
      field: "id",
      headerName: "ID",
      flex: 1,
      minWidth: 200,
    },
    {
      field: "description",
      headerName: "Description",
      flex: 1,
      minWidth: 200,
    },
    {
      field: "created",
      headerName: "Created",
      flex: 0.8,
      minWidth: 150,
      renderCell: (params: GridRenderCellParams<ApiKeyDto>) => {
        return moment(params.row.created).format("YYYY-MM-DD HH:mm");
      },
    },
    {
      field: "devices",
      headerName: "Devices",
      flex: 0.6,
      minWidth: 100,
      renderCell: (params: GridRenderCellParams<ApiKeyDto>) => {
        const deviceIds = getDeviceIds(params.row);
        return deviceIds.length > 0 ? (
          <Chip label={deviceIds.length} size="small" color="primary" />
        ) : (
          <span>-</span>
        );
      },
    },
    {
      field: "locations",
      headerName: "Locations",
      flex: 0.6,
      minWidth: 100,
      renderCell: (params: GridRenderCellParams<ApiKeyDto>) => {
        const locationIds = getLocationIds(params.row);
        return locationIds.length > 0 ? (
          <Chip label={locationIds.length} size="small" color="secondary" />
        ) : (
          <span>-</span>
        );
      },
    },
    {
      field: "actions",
      headerName: "Actions",
      flex: 0.5,
      minWidth: 120,
      sortable: false,
      renderCell: (params: GridRenderCellParams<ApiKeyDto>) => (
        <Box sx={{ display: "flex", gap: 1 }}>
          <Tooltip title="View Details">
            <IconButton
              size="small"
              onClick={(e) => {
                e.stopPropagation();
                onViewDetails(params.row);
              }}
            >
              <Info />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete">
            <IconButton
              size="small"
              color="error"
              onClick={(e) => {
                e.stopPropagation();
                onDelete(params.row);
              }}
            >
              <Delete />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  return (
    <Box sx={{ height: 600, width: "100%" }}>
      <DataGrid
        rows={apiKeys}
        columns={columns}
        loading={isLoading}
        pageSizeOptions={[10, 25, 50, 100]}
        initialState={{
          pagination: {
            paginationModel: { pageSize: 25, page: 0 },
          },
          sorting: {
            sortModel: [{ field: "created", sort: "desc" }],
          },
        }}
        onRowClick={(params: GridRowParams<ApiKeyDto>) => {
          onViewDetails(params.row);
        }}
        sx={{
          "& .MuiDataGrid-row": {
            cursor: "pointer",
          },
        }}
      />
    </Box>
  );
};
