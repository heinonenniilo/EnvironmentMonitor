import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  IconButton,
  TextField,
  Divider,
} from "@mui/material";
import { Close, Visibility } from "@mui/icons-material";
import { type DeviceEmailTemplateDto } from "../../models/deviceEmailTemplate";
import { useState, useEffect } from "react";
import { EmailTemplatePreview } from "./EmailTemplatePreview";

export interface EditEmailTemplateDialogProps {
  open: boolean;
  template: DeviceEmailTemplateDto | null;
  onClose: () => void;
  onSave?: (identifier: string, title: string, message: string) => void;
}

export const EditEmailTemplateDialog: React.FC<
  EditEmailTemplateDialogProps
> = ({ open, template, onClose, onSave }) => {
  const [title, setTitle] = useState("");
  const [message, setMessage] = useState("");
  const [showPreview, setShowPreview] = useState(false);

  useEffect(() => {
    if (template) {
      setTitle(template.title ?? "");
      setMessage(template.message ?? "");
    } else {
      setTitle("");
      setMessage("");
    }
  }, [template, open]);

  const handleSave = () => {
    if (!onSave || !template) {
      return;
    }

    onSave(template.identifier, title.trim(), message.trim());
  };

  const isSaveDisabled = () => {
    if (!title.trim() || !message.trim()) {
      return true;
    }

    // Check if values have changed
    if (template) {
      return (
        title.trim() === (template.title ?? "") &&
        message.trim() === (template.message ?? "")
      );
    }

    return false;
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullScreen
      fullWidth
      sx={{
        padding: 6,
        display: "flex",
        flexDirection: "column",
      }}
    >
      <DialogTitle
        sx={{
          display: "flex",
          flexDirection: "row",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <Box>Edit Email Template: {template?.identifier}</Box>
        <Box display="flex" gap={1} alignItems="center">
          <Button
            startIcon={<Visibility />}
            onClick={() => setShowPreview(!showPreview)}
            variant={showPreview ? "contained" : "outlined"}
            size="small"
          >
            {showPreview ? "Hide Preview" : "Show Preview"}
          </Button>
          <IconButton
            aria-label="close"
            onClick={onClose}
            sx={{
              color: (theme) => theme.palette.grey[500],
            }}
            size="small"
          >
            <Close />
          </IconButton>
        </Box>
      </DialogTitle>
      <DialogContent
        sx={{
          flex: 1,
          overflowY: "auto",
          display: "flex",
          flexDirection: "column",
        }}
      >
        <Box
          display="flex"
          flexDirection={showPreview ? "row" : "column"}
          gap={2}
          mt={1}
          flex={1}
        >
          <Box
            display="flex"
            flexDirection="column"
            gap={2}
            flex={1}
            minWidth={showPreview ? "50%" : "100%"}
          >
            <TextField
              label="Title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              fullWidth
              required
            />
            <TextField
              label="Message"
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              fullWidth
              required
              multiline
              sx={{ flex: 1 }}
              helperText="You can use HTML formatting in the message"
            />
          </Box>

          {showPreview && (
            <>
              <Divider orientation="vertical" flexItem />
              <EmailTemplatePreview title={title} message={message} />
            </>
          )}
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="inherit">
          Cancel
        </Button>
        <Button
          onClick={handleSave}
          color="primary"
          variant="contained"
          disabled={isSaveDisabled()}
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};
