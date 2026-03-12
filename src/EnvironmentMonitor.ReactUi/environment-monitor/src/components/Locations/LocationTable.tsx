import { Checkbox, IconButton } from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { Delete } from "@mui/icons-material";
import { Link } from "react-router";
import type { LocationModel } from "../../models/location";
import { routes } from "../../utilities/routes";
import { getEntityTitle } from "../../utilities/entityUtils";

export interface LocationTableProps {
  locations: LocationModel[];
  renderLink?: boolean;
  onDelete?: (location: LocationModel) => void;
}

export const LocationTable: React.FC<LocationTableProps> = ({
  locations,
  renderLink,
  onDelete,
}) => {
  const columns: GridColDef[] = [
    {
      field: "name",
      headerName: "Name",
      flex: 1,
      minWidth: 220,
      renderCell: (params) => {
        const location = params.row as LocationModel;
        const title = getEntityTitle(location);

        if (!renderLink) {
          return title;
        }

        return (
          <Link to={`${routes.locations}/${location.identifier}`}>{title}</Link>
        );
      },
      valueGetter: (_value, row) => getEntityTitle(row as LocationModel),
    },
    {
      field: "identifier",
      headerName: "Identifier",
      flex: 1,
      minWidth: 240,
      valueGetter: (_value, row) => (row as LocationModel).identifier,
    },
    {
      field: "visible",
      headerName: "Visible",
      width: 80,
      renderCell: (params) => (
        <Checkbox
          checked={(params.row as LocationModel).visible}
          size="small"
          disabled
          sx={{ padding: 0 }}
        />
      ),
    },
  ];

  if (onDelete) {
    columns.push({
      field: "actions",
      headerName: "",
      width: 70,
      sortable: false,
      filterable: false,
      align: "right",
      renderCell: (params) => (
        <IconButton
          size="small"
          color="error"
          title="Delete location"
          onClick={() => onDelete(params.row as LocationModel)}
        >
          <Delete fontSize="small" />
        </IconButton>
      ),
    });
  }

  return (
    <DataGrid
      rows={locations}
      columns={columns}
      getRowId={(row) => (row as LocationModel).identifier}
      hideFooter
      pageSizeOptions={[]}
      disableRowSelectionOnClick
      sx={{
        border: 0,
        minWidth: 700,
      }}
      initialState={{
        sorting: {
          sortModel: [{ field: "name", sort: "asc" }],
        },
      }}
    />
  );
};
