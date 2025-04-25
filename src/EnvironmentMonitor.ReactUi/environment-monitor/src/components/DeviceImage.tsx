import {
  Box,
  Button,
  CircularProgress,
  IconButton,
  Typography,
} from "@mui/material";
import { useRef, useState } from "react";
import {
  ArrowBack,
  ArrowForward,
  Delete,
  FileUpload,
} from "@mui/icons-material";
import { DeviceInfo } from "../models/deviceInfo";

export interface DeviceImageProps {
  device: DeviceInfo | undefined;
  title?: string;
  onUploadImage: (file: File) => void;
  onDeleteImage: (identifier: string) => void;
  ver?: number;
}

export const DeviceImage: React.FC<DeviceImageProps> = ({
  device,
  title,
  onUploadImage,
  onDeleteImage,
  ver,
}) => {
  const [isLoadingImage, setIsLoadingImage] = useState(true);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [currentIndex, setCurrentIndex] = useState(0);

  const prevImage = () => {
    if (imageUrls.length <= 1) {
      return;
    }
    setCurrentIndex((prev) => (prev - 1 + imageUrls.length) % imageUrls.length);
    setIsLoadingImage(true);
  };

  const nextImage = () => {
    if (imageUrls.length <= 1) {
      return;
    }
    setCurrentIndex((prev) => (prev + 1) % imageUrls.length);
    setIsLoadingImage(true);
  };

  const openFileDialog = () => {
    fileInputRef.current?.click();
  };

  const imageUrls =
    device?.attachments.map(
      (d) =>
        `/api/devices/attachment/${device?.device.deviceIdentifier}/${d.guid}`
    ) ?? [];
  return !device ? (
    <></>
  ) : (
    <Box marginTop={2} display={"flex"} flexDirection={"row"}>
      {imageUrls.length > 0 ? (
        <Box
          sx={{
            maxHeight: 600,
            position: "relative",
            display: "flex",
            flexDirection: "column",
            justifyContent: "flex-start",
          }}
        >
          <Typography variant="h6" marginBottom={0}>
            {title ?? "Devices images"}
          </Typography>
          <Box sx={{ display: "flex", flexDirection: "row" }}>
            <IconButton
              onClick={openFileDialog}
              sx={{ ml: 1, cursor: "pointer" }}
              size="small"
            >
              <FileUpload />
            </IconButton>
            <IconButton
              onClick={() => {
                const attachment = device.attachments[currentIndex];
                onDeleteImage(attachment.guid);
              }}
              sx={{ ml: 1, cursor: "pointer" }}
              size="small"
            >
              <Delete />
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
            src={imageUrls[currentIndex]}
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
          <Box
            sx={{
              display: "flex",
              flexDirection: "row",
              justifyContent: "space-between",
            }}
          >
            <IconButton
              onClick={prevImage}
              sx={{ ml: 1, cursor: "pointer" }}
              size="small"
            >
              <ArrowBack />
            </IconButton>
            <Typography variant="caption">{`${currentIndex + 1}/${
              imageUrls.length
            }`}</Typography>
            <IconButton
              onClick={nextImage}
              sx={{ ml: 1, cursor: "pointer" }}
              size="small"
            >
              <ArrowForward />
            </IconButton>
          </Box>
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
