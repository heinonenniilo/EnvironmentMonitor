import { DeviceInfo } from "../models/deviceInfo";
import { Box, Checkbox, Typography } from "@mui/material";
import { routes } from "../utilities/routes";
import { Link } from "react-router";
import { getFormattedDate } from "../utilities/datetimeUtils";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { CheckCircle, WarningAmber } from "@mui/icons-material";

export interface DeviceTableProps {
  devices: DeviceInfo[];
  onReboot?: (device: DeviceInfo) => void;
  title?: string;
  disableSort?: boolean;
  hideName?: boolean;
}

export const DeviceTable: React.FC<DeviceTableProps> = ({
  devices,
  title,
  disableSort,
  hideName,
}) => {
  const formatDate = (input: Date | undefined | null) => {
    if (input) {
      return getFormattedDate(input, true);
    } else {
      return "";
    }
  };
  const columns: GridColDef[] = [
    {
      field: "name",
      headerName: "Name",
      hideable: true,
      flex: 1,
      renderCell: (params) => (
        <Link
          to={`${routes.devices}/${
            (params?.row as DeviceInfo)?.device.deviceIdentifier
          }`}
        >
          {(params?.row as DeviceInfo)?.device.name}
        </Link>
      ),
      valueGetter: (value, row) => {
        if (!row) {
          return "";
        }
        return (row as DeviceInfo)?.device.name;
      },
    },
    {
      field: "visible",
      headerName: "Visible",
      width: 60,
      valueGetter: (value, row) => {
        if (!row) {
          return false;
        }
        return (row as DeviceInfo).device.visible;
      },
      renderCell: (params) => {
        return (
          <Checkbox
            checked={(params?.row as DeviceInfo)?.device.visible}
            size="small"
            disabled
            sx={{ padding: "0px" }}
          />
        );
      },
    },
    {
      field: "onlineSince",
      headerName: "Online since",
      type: "dateTime",
      flex: 1,
      valueGetter: (value, row) => {
        if (row) {
          const data = row as DeviceInfo;
          if (data.onlineSince) {
            return new Date(data.onlineSince);
          }
        }
        return null;
      },
      valueFormatter: (value, row) => {
        return formatDate((row as DeviceInfo)?.onlineSince);
      },
    },
    {
      field: "rebootedOn",
      headerName: "Rebooted",
      type: "dateTime",
      flex: 1,
      valueGetter: (value, row) => {
        if (row) {
          const data = row as DeviceInfo;
          if (data.rebootedOn) {
            return new Date(data.rebootedOn);
          }
        }
        return null;
      },
      valueFormatter: (value, row) => {
        return formatDate((row as DeviceInfo)?.rebootedOn);
      },
    },
    {
      field: "lastMessage",
      headerName: "Last Message",
      type: "dateTime",
      flex: 1,
      valueGetter: (value, row) => {
        if (row) {
          const data = row as DeviceInfo;
          if (data.lastMessage) {
            return new Date(data.lastMessage);
          }
        }
        return null;
      },
      valueFormatter: (value, row) => {
        return formatDate((row as DeviceInfo)?.lastMessage);
      },
      renderCell: (params) => {
        const row = params.row as DeviceInfo;

        return (
          <div
            style={{
              display: "flex",
              alignItems: "center",
              gap: "4px",

              padding: "2px",
              borderRadius: "4px",
            }}
          >
            {row.showWarning ? (
              <WarningAmber color="warning" fontSize="small" />
            ) : (
              <CheckCircle color="success" />
            )}
            <span>{formatDate(params.row.lastMessage)}</span>
          </div>
        );
      },
    },
  ];

  return (
    <Box marginTop={2} sx={{ overflow: "auto", minWidth: "600" }}>
      {title !== undefined ? (
        <Typography variant="h6" marginBottom={2}>
          {title}
        </Typography>
      ) : null}
      <DataGrid
        rows={devices}
        columns={columns}
        getRowId={(row) => {
          if (row) {
            return (row as DeviceInfo).device.id;
          }
          return "";
        }}
        sx={{
          border: 0,
          minWidth: 600,
        }}
        pagination={undefined}
        pageSizeOptions={[]}
        disableColumnSorting={disableSort}
        hideFooter
        initialState={{
          columns: {
            columnVisibilityModel: hideName
              ? {
                  name: false,
                }
              : undefined,
          },
          sorting: disableSort
            ? undefined
            : {
                sortModel: [
                  {
                    field: "onlineSince",
                    sort: "desc",
                  },
                ],
              },
        }}
      />
    </Box>
  );
};
