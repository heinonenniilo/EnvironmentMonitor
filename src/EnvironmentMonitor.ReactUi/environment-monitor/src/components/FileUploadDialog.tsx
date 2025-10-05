import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
  Typography,
} from "@mui/material";
import { useState, useEffect } from "react";
import { formatBytes } from "../utilities/stringUtils";

export interface FileUploadDialogProps {
  open: boolean;
  file: File | null;
  onClose: () => void;
  onConfirm: (file: File, customName?: string) => void;
  title?: string;
}

export const FileUploadDialog: React.FC<FileUploadDialogProps> = ({
  open,
  file,
  onClose,
  onConfirm,
  title = "Upload File",
}) => {
  const [customName, setCustomName] = useState("");

  useEffect(() => {
    if (file) {
      setCustomName(file.name);
    }
  }, [file]);

  const handleConfirm = () => {
    if (file) {
      const finalName = customName.trim() || file.name;
      onConfirm(file, finalName);
      handleClose();
    }
  };

  const handleClose = () => {
    setCustomName("");
    onClose();
  };

  const getFileExtension = (filename: string) => {
    return filename.split(".").pop() || "";
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <Box display="flex" flexDirection="column" gap={2} mt={1}>
          <Typography variant="body2" color="textSecondary">
            Original filename: {file?.name}
          </Typography>
          <Typography variant="body2" color="textSecondary">
            File size: {file ? formatBytes(file.size) : "0 B"}
          </Typography>
          <TextField
            fullWidth
            label="File name to save"
            value={customName}
            onChange={(e) => setCustomName(e.target.value)}
            placeholder={file?.name || "Enter custom name"}
            helperText={`Extension: .${
              file ? getFileExtension(file.name) : ""
            }`}
            variant="outlined"
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button onClick={handleConfirm} color="primary" variant="contained">
          Upload
        </Button>
      </DialogActions>
    </Dialog>
  );
};
