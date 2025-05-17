import { type DeviceInfo } from "../models/deviceInfo";
import { Box, Checkbox, IconButton, Tooltip, Typography } from "@mui/material";
import { routes } from "../utilities/routes";
import { Link } from "react-router";
import { getFormattedDate } from "../utilities/datetimeUtils";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { CheckCircle, Photo, WarningAmber } from "@mui/icons-material";
import { useState } from "react";
import { DeviceImageDialog } from "./DeviceImageDialog";

export interface DeviceTableProps {
  devices: DeviceInfo[];
  onReboot?: (device: DeviceInfo) => void;
  onClickVisible?: (device: DeviceInfo) => void;
  title?: string;
  disableSort?: boolean;
  showDeviceImageAsTooltip?: boolean;
  hideName?: boolean;
  hideId?: boolean;
  renderLink?: boolean;
}

export const DeviceTable: React.FC<DeviceTableProps> = ({
  devices,
  title,
  disableSort,
  hideName,
  hideId,
  showDeviceImageAsTooltip,
  renderLink,
  onClickVisible,
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
      field: "id",
      headerName: "Id",
      hideable: true,
      flex: 1,
      maxWidth: 70,
      renderCell: (params) =>
        renderLink ? (
          <Link
            to={`${routes.devices}/${
              (params?.row as DeviceInfo)?.device.identifier
            }`}
          >
            {(params?.row as DeviceInfo)?.device.id}
          </Link>
        ) : (
          (params?.row as DeviceInfo)?.device.id
        ),
      valueGetter: (_value, row) => {
        if (!row) {
          return "";
        }
        return (row as DeviceInfo)?.device.id;
      },
    },
    {
      field: "name",
      headerName: "Name",
      hideable: true,
      flex: 1,
      minWidth: 200,
      renderCell: (params) => {
        const text =
          (params?.row as DeviceInfo)?.device.displayName ??
          (params?.row as DeviceInfo)?.device.name;
        return renderLink ? (
          <Link
            to={`${routes.devices}/${
              (params?.row as DeviceInfo)?.device.identifier
            }`}
            onClick={() => {}}
          >
            {text}
          </Link>
        ) : (
          text
        );
      },
    },
    {
      field: "image",
      headerName: "Image",
      sortable: false,
      filterable: false,
      renderCell: (params) => {
        const device = params.row as DeviceInfo;

        if (!device.defaultImageGuid) {
          return null;
        }
        const imageUrl = `/api/Devices/default-image/${device.device.identifier}`;
        const iconButtonToRender = (
          <IconButton
            onClick={() => {
              setSelectedDeviceIdentifier(device.device.identifier);
            }}
          >
            <Photo />
          </IconButton>
        );

        return showDeviceImageAsTooltip ? (
          <Tooltip
            title={
              <Box
                component="img"
                src={imageUrl}
                alt="Preview"
                sx={{
                  maxHeight: 400,
                  transition: "filter 0.3s ease-in-out",
                  borderRadius: 1,
                  display: "block",
                }}
              />
            }
            followCursor
            arrow
          >
            {iconButtonToRender}
          </Tooltip>
        ) : (
          iconButtonToRender
        );
      },
    },
    {
      field: "visible",
      headerName: "Visible",
      width: 60,
      valueGetter: (_value, row) => {
        if (!row) {
          return false;
        }
        return (row as DeviceInfo).device.visible;
      },
      renderCell: (params) => {
        const checkBox = (
          <Checkbox
            checked={(params?.row as DeviceInfo)?.device.visible}
            size="small"
            disabled
            sx={{
              padding: "0px",
              cursor: "pointer",
            }}
          />
        );
        if (!onClickVisible) {
          return checkBox;
        }
        return (
          <IconButton
            onClick={() => {
              if (onClickVisible) {
                onClickVisible(params.row as DeviceInfo);
              } else {
                console.info("No data");
              }
            }}
          >
            {checkBox}
          </IconButton>
        );
      },
    },
    {
      field: "onlineSince",
      headerName: "Online since",
      type: "dateTime",
      flex: 1,
      minWidth: 170,
      valueGetter: (_value, row) => {
        if (row) {
          const data = row as DeviceInfo;
          if (data.onlineSince) {
            return new Date(data.onlineSince);
          }
        }
        return null;
      },
      valueFormatter: (_value, row) => {
        return formatDate((row as DeviceInfo)?.onlineSince);
      },
    },
    {
      field: "rebootedOn",
      headerName: "Rebooted",
      type: "dateTime",
      flex: 1,
      minWidth: 170,
      valueGetter: (_value, row) => {
        if (row) {
          const data = row as DeviceInfo;
          if (data.rebootedOn) {
            return new Date(data.rebootedOn);
          }
        }
        return null;
      },
      valueFormatter: (_value, row) => {
        return formatDate((row as DeviceInfo)?.rebootedOn);
      },
    },
    {
      field: "lastMessage",
      headerName: "Last Message",
      type: "dateTime",
      flex: 1,
      minWidth: 170,
      valueGetter: (_value, row) => {
        if (row) {
          const data = row as DeviceInfo;
          if (data.lastMessage) {
            return new Date(data.lastMessage);
          }
        }
        return null;
      },
      valueFormatter: (_value, row) => {
        return formatDate((row as DeviceInfo)?.lastMessage);
      },
      renderCell: (params) => {
        const row = params.row as DeviceInfo;

        return (
          <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
            {row.showWarning ? (
              <WarningAmber color="warning" />
            ) : (
              <CheckCircle color="success" />
            )}
            <span>{formatDate(params.row.lastMessage)}</span>
          </Box>
        );
      },
    },
  ];

  const [selectedDeviceIdentifier, setSelectedDeviceIdentifier] = useState<
    string | undefined
  >(undefined);

  const selectedDevice =
    selectedDeviceIdentifier !== undefined
      ? devices.find((d) => d.device.identifier === selectedDeviceIdentifier)
      : undefined;

  return (
    <Box marginTop={1} display={"flex"} flexDirection={"column"}>
      {title !== undefined ? (
        <Typography variant="h6" marginBottom={2}>
          {title}
        </Typography>
      ) : null}
      <DeviceImageDialog
        isOpen={
          selectedDeviceIdentifier !== undefined &&
          selectedDeviceIdentifier.length > 0
        }
        imageUrl={`/api/Devices/default-image/${selectedDeviceIdentifier}`}
        onClose={() => {
          setSelectedDeviceIdentifier("");
        }}
        title={selectedDevice?.device.name}
      />
      <Box sx={{ overflow: "auto" }}>
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
              columnVisibilityModel:
                hideName || hideId
                  ? {
                      name: hideName ? false : true,
                      id: hideId ? false : true,
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
    </Box>
  );
};
