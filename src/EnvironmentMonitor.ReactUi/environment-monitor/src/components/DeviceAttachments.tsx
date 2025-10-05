import { Box, IconButton, Tooltip, Typography } from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { Collapsible } from "./CollabsibleComponent";
import type { DeviceAttachment } from "../models/deviceAttachment";
import { getFormattedDate } from "../utilities/datetimeUtils";
import { formatBytes } from "../utilities/stringUtils";
import { FileUpload, Download, Delete } from "@mui/icons-material";
import { useRef, useState } from "react";
import type { DeviceInfo } from "../models/deviceInfo";
import { getDeviceAttachmentUrl } from "../utilities/deviceUtils";
import { FileUploadDialog } from "./FileUploadDialog";

export interface DeviceAttachmentsProps {
  attachments: DeviceAttachment[];
  device: DeviceInfo | undefined;
  title?: string;
  onUploadAttachment: (file: File, customName?: string) => void;
  onDeleteAttachment: (attachmentId: string) => void;
}

export const DeviceAttachments: React.FC<DeviceAttachmentsProps> = ({
  attachments,
  device,
  title,
  onUploadAttachment,
  onDeleteAttachment,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [uploadDialogOpen, setUploadDialogOpen] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const columns: GridColDef[] = [
    {
      field: "name",
      headerName: "Name",
      flex: 1,
      sortable: false,
      minWidth: 150,
    },
    {
      field: "isImage",
      headerName: "Type",
      flex: 1,
      sortable: false,
      minWidth: 100,
      valueFormatter: (value) => {
        return value ? "Image" : "File";
      },
    },
    {
      field: "sizeInBytes",
      headerName: "Size",
      flex: 1,
      sortable: false,
      minWidth: 100,
      valueFormatter: (value) => {
        return formatBytes(value as number);
      },
    },
    {
      field: "created",
      headerName: "Created",
      flex: 1,
      sortable: false,
      minWidth: 150,
      valueFormatter: (value) => {
        if (!value) {
          return "-";
        }
        return getFormattedDate(value as Date, true, true);
      },
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 120,
      sortable: false,
      disableColumnMenu: true,
      renderCell: (params) => {
        const attachment = params.row as DeviceAttachment;
        return (
          <Box sx={{ display: "flex", gap: 0.5 }}>
            <Tooltip title="Download attachment">
              <IconButton
                size="small"
                onClick={() => {
                  // Create download URL using attachment guid
                  const downloadUrl = getDeviceAttachmentUrl(
                    device?.device.identifier ?? "",
                    attachment.guid
                  );
                  console.log("Downloading from URL:", downloadUrl);
                  const link = document.createElement("a");
                  link.href = downloadUrl;
                  link.download = attachment.name;
                  document.body.appendChild(link);
                  link.click();
                  document.body.removeChild(link);
                }}
              >
                <Download />
              </IconButton>
            </Tooltip>
            <Tooltip title="Delete attachment">
              <IconButton
                size="small"
                onClick={() => {
                  onDeleteAttachment(attachment.guid);
                }}
              >
                <Delete />
              </IconButton>
            </Tooltip>
          </Box>
        );
      },
    },
  ];

  const openFileDialog = () => {
    fileInputRef.current?.click();
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setSelectedFile(e.target.files[0]);
      setUploadDialogOpen(true);
      // Reset input value to allow selecting same file again
      e.target.value = "";
    }
  };

  const handleUploadConfirm = (file: File, customName?: string) => {
    onUploadAttachment(file, customName);
    setSelectedFile(null);
    setUploadDialogOpen(false);
  };

  const handleUploadCancel = () => {
    setSelectedFile(null);
    setUploadDialogOpen(false);
  };

  return (
    <>
      <Collapsible
        title={title ?? "Attachments"}
        isOpen={true}
        customComponent={
          <Tooltip title="Upload new attachment" arrow>
            <IconButton
              onClick={openFileDialog}
              sx={{ ml: 1, cursor: "pointer" }}
              size="small"
            >
              <FileUpload />
            </IconButton>
          </Tooltip>
        }
      >
        <Box marginTop={2} display="flex" flexDirection="column">
          {attachments.length === 0 ? (
            <Typography variant="body2" color="textSecondary">
              No attachments found
            </Typography>
          ) : (
            <Box sx={{ width: "100%", maxHeight: 600 }}>
              <DataGrid
                rows={attachments}
                columns={columns}
                getRowId={(row) => (row as DeviceAttachment).guid}
                disableRowSelectionOnClick
                density="compact"
                hideFooter={attachments.length <= 25}
                pageSizeOptions={[25, 50, 100]}
                autoHeight
                sx={{
                  maxHeight: 600,
                  "& .MuiDataGrid-virtualScroller": {
                    maxHeight: 550,
                  },
                }}
              />
            </Box>
          )}
          <input
            type="file"
            hidden
            ref={fileInputRef}
            onChange={handleFileSelect}
          />
        </Box>
      </Collapsible>

      <FileUploadDialog
        open={uploadDialogOpen}
        file={selectedFile}
        onClose={handleUploadCancel}
        onConfirm={handleUploadConfirm}
        title="Upload Attachment"
      />
    </>
  );
};
