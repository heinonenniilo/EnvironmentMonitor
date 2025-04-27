import { Close } from "@mui/icons-material";
import {
  Box,
  Dialog,
  DialogContent,
  DialogTitle,
  IconButton,
  useMediaQuery,
  useTheme,
} from "@mui/material";

export interface DeviceImageDialogProps {
  title?: string;
  isOpen: boolean;
  imageUrl: string;
  onClose: () => void;
}

export const DeviceImageDialog: React.FC<DeviceImageDialogProps> = ({
  imageUrl,
  title,
  isOpen,
  onClose,
}) => {
  const theme = useTheme();
  const drawTitle = useMediaQuery(theme.breakpoints.up("lg"));

  return (
    <Dialog open={isOpen} onClose={onClose} maxWidth="xl">
      {drawTitle && (
        <DialogTitle
          sx={{
            display: "flex",
            flexDirection: "row",
            justifyContent: "space-between",
          }}
        >
          <Box>{title ?? ""}</Box>
          <Box sx={{ display: "flex", flexBasis: "row" }}>
            <IconButton
              aria-label="close"
              onClick={() => {
                onClose();
              }}
              sx={{
                color: (theme) => theme.palette.grey[500],
              }}
              size="small"
            >
              <Close />
            </IconButton>
          </Box>
        </DialogTitle>
      )}

      <DialogContent>
        <Box
          component="img"
          src={imageUrl}
          alt="Preview"
          sx={{
            width: "100%",
            height: "auto",
            borderRadius: 1,
            display: "block",
          }}
        />
      </DialogContent>
    </Dialog>
  );
};
