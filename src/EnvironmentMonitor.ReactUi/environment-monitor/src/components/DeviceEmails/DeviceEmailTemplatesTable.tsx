import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { type DeviceEmailTemplateDto } from "../../models/deviceEmailTemplate";
import { IconButton } from "@mui/material";
import { Edit } from "@mui/icons-material";
import { getFormattedDate } from "../../utilities/datetimeUtils";

export interface DeviceEmailTemplatesTableProps {
  templates: DeviceEmailTemplateDto[];
  onEdit?: (template: DeviceEmailTemplateDto) => void;
}

export const DeviceEmailTemplatesTable: React.FC<
  DeviceEmailTemplatesTableProps
> = ({ templates, onEdit }) => {
  const formatDate = (input: Date | undefined | null) => {
    if (input) {
      return getFormattedDate(input, true);
    } else {
      return "";
    }
  };

  const columns: GridColDef[] = [
    {
      field: "identifier",
      headerName: "Template",
      flex: 1,
      minWidth: 300,
      valueGetter: (_value, row) => {
        if (!row) {
          return "";
        }
        return (row as DeviceEmailTemplateDto)?.displayName;
      },
    },
    {
      field: "updated",
      headerName: "Updated",
      flex: 1,
      minWidth: 150,
      valueGetter: (_value, row) => {
        if (!row) {
          return "";
        }
        return formatDate((row as DeviceEmailTemplateDto)?.updated);
      },
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 100,
      sortable: false,
      renderCell: (params) => (
        <IconButton
          size="small"
          color="primary"
          onClick={() => {
            if (onEdit) {
              onEdit(params.row as DeviceEmailTemplateDto);
            }
          }}
        >
          <Edit />
        </IconButton>
      ),
    },
  ];

  return (
    <DataGrid
      rows={templates}
      columns={columns}
      getRowId={(row) => row.identifier}
      disableRowSelectionOnClick
      autoHeight
      initialState={{
        pagination: {
          paginationModel: { pageSize: 10 },
        },
      }}
      pageSizeOptions={[5, 10, 25]}
    />
  );
};
