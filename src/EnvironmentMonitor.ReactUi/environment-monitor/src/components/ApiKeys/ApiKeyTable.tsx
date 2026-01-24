import React from "react";
import {
  DataGrid,
  type GridColDef,
  type GridRenderCellParams,
} from "@mui/x-data-grid";
import { IconButton, Box, Tooltip, Switch } from "@mui/material";
import { Delete, Info } from "@mui/icons-material";
import type { ApiKeyDto } from "../../models/apiKey";
import moment from "moment";

interface ApiKeyTableProps {
  apiKeys: ApiKeyDto[];
  onViewDetails: (apiKey: ApiKeyDto) => void;
  onDelete: (apiKey: ApiKeyDto) => void;
  onToggleEnabled: (apiKey: ApiKeyDto) => void;
  isLoading?: boolean;
}

export const ApiKeyTable: React.FC<ApiKeyTableProps> = ({
  apiKeys,
  onViewDetails,
  onDelete,
  onToggleEnabled,
  isLoading,
}) => {
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
      field: "enabled",
      headerName: "Enabled",
      flex: 0.4,
      minWidth: 100,
      renderCell: (params: GridRenderCellParams<ApiKeyDto>) => (
        <Switch
          checked={params.row.enabled}
          onChange={(e) => {
            e.stopPropagation();
            onToggleEnabled(params.row);
          }}
          color="primary"
          size="small"
        />
      ),
    },
    {
      field: "actions",
      headerName: "Actions",
      flex: 0.5,
      minWidth: 120,
      sortable: false,
      renderCell: (params: GridRenderCellParams<ApiKeyDto>) => (
        <Box
          sx={{ display: "flex", gap: 1, alignItems: "center", height: "100%" }}
        >
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
      />
    </Box>
  );
};
