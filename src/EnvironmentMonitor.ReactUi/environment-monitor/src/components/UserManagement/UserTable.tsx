import React from "react";
import { Box, Checkbox, Typography } from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { Link } from "react-router";
import { routes } from "../../utilities/routes";
import type { UserInfoDto } from "../../models/userInfoDto";
import { getFormattedDate } from "../../utilities/datetimeUtils";

export interface UserTableProps {
  users: UserInfoDto[];
  title?: string;
  renderLink?: boolean;
}

export const UserTable: React.FC<UserTableProps> = ({
  users,
  title,
  renderLink = true,
}) => {
  const columns: GridColDef[] = [
    {
      field: "email",
      headerName: "Email",
      flex: 1,
      minWidth: 200,
      renderCell: (params) => {
        const user = params.row as UserInfoDto;
        return renderLink ? (
          <Link to={`${routes.users}/${user.id}`}>{user.email}</Link>
        ) : (
          user.email
        );
      },
      valueGetter: (_value, row) => {
        if (!row) {
          return "";
        }
        return (row as UserInfoDto).email;
      },
    },
    {
      field: "updated",
      headerName: "Last Updated",
      flex: 1,
      minWidth: 200,
      renderCell: (params) => {
        const user = params.row as UserInfoDto;
        if (!user.updated) return "";

        const formattedDate = getFormattedDate(user.updated, true);
        return formattedDate;
      },
      valueGetter: (_value, row) => {
        if (!row) {
          return "";
        }
        return (row as UserInfoDto).updated || "";
      },
    },
    {
      field: "roles",
      headerName: "Roles",
      flex: 1,
      minWidth: 150,
      sortable: false,
      renderCell: (params) => {
        const user = params.row as UserInfoDto;
        return user.roles.join(", ");
      },
    },
    {
      field: "emailConfirmed",
      headerName: "Email Confirmed",
      width: 140,
      renderCell: (params) => {
        const user = params.row as UserInfoDto;
        return (
          <Checkbox
            checked={user.emailConfirmed}
            size="small"
            disabled
            sx={{
              padding: "0px",
            }}
          />
        );
      },
      valueGetter: (_value, row) => {
        if (!row) {
          return false;
        }
        return (row as UserInfoDto).emailConfirmed;
      },
    },
  ];

  return (
    <Box sx={{ width: "100%" }}>
      {title && (
        <Typography variant="h6" sx={{ mb: 2 }}>
          {title}
        </Typography>
      )}
      <DataGrid
        rows={users}
        columns={columns}
        initialState={{
          pagination: {
            paginationModel: { pageSize: 25, page: 0 },
          },
        }}
        pageSizeOptions={[10, 25, 50, 100]}
        disableRowSelectionOnClick
        getRowId={(row) => row.id}
        autoHeight
        sx={{
          "& .MuiDataGrid-cell": {
            py: 1,
          },
        }}
      />
    </Box>
  );
};
