import {
  Box,
  Button,
  CircularProgress,
  IconButton,
  Typography,
} from "@mui/material";
import { Device } from "../models/device";
import { useRef, useState } from "react";
import { FileUpload } from "@mui/icons-material";

export interface DeviceImageProps {
  device: Device | undefined;
  title: string;
  onUploadImage: (file: File) => void;
  ver?: number;
}

export const DeviceImage: React.FC<DeviceImageProps> = ({
  device,
  title,
  onUploadImage,
  ver,
}) => {
  const [isLoadingImage, setIsLoadingImage] = useState(true);
  const fileInputRef = useRef<HTMLInputElement>(null);
  // const [file, setFile] = useState<File | null>(null);
  if (!device) {
    return <></>;
  }

  const imageUrl = `/api/devices/default-image/${device.deviceIdentifier}?ver=${
    ver ?? 0
  }`;

  const openFileDialog = () => {
    fileInputRef.current?.click();
  };
  return (
    <Box marginTop={2} display={"flex"} flexDirection={"row"}>
      {device.hasDefaultImage ? (
        <Box
          sx={{ maxHeight: 600, position: "relative", alignItems: "center" }}
        >
          <Box sx={{ display: "flex", flexDirection: "row" }}>
            <Typography variant="h6" marginBottom={0}>
              {title ?? "Image"}
            </Typography>
            <IconButton
              onClick={openFileDialog}
              sx={{ ml: 1, cursor: "pointer" }}
              size="small"
            >
              <FileUpload></FileUpload>
            </IconButton>
          </Box>
          {isLoadingImage && (
            <Box
              sx={{
                position: "absolute",
                inset: 0,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                backgroundColor: "rgba(255,255,255,0.6)",
                zIndex: 1,
              }}
            >
              <CircularProgress />
            </Box>
          )}
          <img
            src={imageUrl}
            alt="Device"
            style={{
              width: "100%",
              display: "block",
              height: "auto",
              maxHeight: 500,
              objectFit: "contain",
              filter: isLoadingImage ? "blur(10px)" : "none",
              transition: "filter 0.3s ease-in-out",
              // maxHeight: 400,
            }}
            onLoad={() => setIsLoadingImage(false)}
            onError={() => {
              setIsLoadingImage(false);
            }}
          />
        </Box>
      ) : (
        <Button
          variant="contained"
          component="label"
          sx={{ mt: 1, mb: 2 }}
          onClick={openFileDialog}
        >
          Choose Image
        </Button>
      )}
      <input
        type="file"
        hidden
        accept="image/*"
        ref={fileInputRef}
        onChange={(e) => {
          if (e.target.files && e.target.files?.length > 0) {
            onUploadImage(e.target.files?.[0]);
          }
        }}
      />
    </Box>
  );
};
