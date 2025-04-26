import {
  Box,
  Button,
  CircularProgress,
  IconButton,
  Tooltip,
  Typography,
} from "@mui/material";
import { useEffect, useRef, useState } from "react";
import {
  ArrowBack,
  ArrowForward,
  CheckCircle,
  Delete,
  FileUpload,
} from "@mui/icons-material";
import { DeviceInfo } from "../models/deviceInfo";
import { Collapsible } from "./CollabsibleComponent";
import { useSwipeable } from "react-swipeable";
import { useDropzone } from "react-dropzone";

export interface DeviceImageProps {
  device: DeviceInfo | undefined;
  title?: string;
  onUploadImage: (file: File) => void;
  onDeleteImage: (identifier: string) => void;
  onSetDefaultImage: (identifier: string) => void;
  ver?: number;
}

export const DeviceImage: React.FC<DeviceImageProps> = ({
  device,
  title,
  onUploadImage,
  onDeleteImage,
  onSetDefaultImage,
  ver,
}) => {
  const [isLoadingImage, setIsLoadingImage] = useState(true);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [currentIndex, setCurrentIndex] = useState(0);

  const { getRootProps, getInputProps } = useDropzone({
    onDrop: (files) => {
      if (files.length === 1) {
        onUploadImage(files[0]);
      }
    },
    noClick: true,
    maxFiles: 1,
  });

  const prevImage = () => {
    if (urls.length <= 1) {
      return;
    }
    setCurrentIndex((prev) => (prev - 1 + urls.length) % urls.length);
    setIsLoadingImage(true);
  };

  const nextImage = () => {
    if (urls.length <= 1) {
      return;
    }
    setCurrentIndex((prev) => (prev + 1) % urls.length);
    setIsLoadingImage(true);
  };

  useEffect(() => {
    if (!device) {
      return;
    }
    if (currentIndex >= device.attachments.length) {
      setCurrentIndex(0);
    }
  }, [device, currentIndex]);

  const handlers = useSwipeable({
    onSwipedLeft: () => nextImage(),
    onSwipedRight: () => prevImage(),
    trackMouse: true,
  });
  const openFileDialog = () => {
    fileInputRef.current?.click();
  };

  const urls =
    device?.attachments.map((s) => {
      return {
        url: `/api/devices/attachment/${device?.device.deviceIdentifier}/${s.guid}`,
        guid: s.guid,
      };
    }) ?? [];

  const isDisabled = (guid: string) => {
    return (
      device === undefined ||
      (device.defaultImageGuid.length > 0 && device.defaultImageGuid === guid)
    );
  };

  return !device ? (
    <></>
  ) : (
    <Collapsible
      title="Device images"
      isOpen={true}
      customComponent={
        <Tooltip title="Upload new image" arrow>
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
      <Box
        marginTop={2}
        display={"flex"}
        flexDirection={"row"}
        {...getRootProps()}
      >
        <input {...getInputProps()} />
        {urls !== undefined && urls.length > 0 ? (
          <Box
            sx={{
              maxHeight: 600,
              position: "relative",

              display: "flex",
              flexDirection: "column",
              justifyContent: "flex-start",
              touchAction: "pan-y",
            }}
            {...handlers}
          >
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
              src={urls[currentIndex].url}
              alt="Device"
              style={{
                width: "100%",
                display: "block",
                height: "auto",
                maxHeight: 500,
                objectFit: "contain",
                filter: isLoadingImage ? "blur(10px)" : "none",
                transition: "filter 0.3s ease-in-out",
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
                alignItems: "center",
              }}
            >
              <IconButton
                onClick={prevImage}
                sx={{ ml: 1, cursor: "pointer" }}
                size="small"
              >
                <ArrowBack />
              </IconButton>
              <Box
                sx={{
                  display: "flex",
                  flexDirection: "row",
                  alignItems: "center",
                }}
              >
                <Typography variant="caption" textAlign={"center"}>{`${
                  currentIndex + 1
                }/${urls.length}`}</Typography>
                <Tooltip title={"Delete image?"}>
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
                </Tooltip>
                <Tooltip title={"Set as default"}>
                  <IconButton
                    onClick={() => {
                      const attachment = device.attachments[currentIndex];
                      onSetDefaultImage(attachment.guid);
                    }}
                    disabled={isDisabled(urls[currentIndex].guid)}
                    sx={{ ml: 1, cursor: "pointer" }}
                    size="small"
                  >
                    <CheckCircle />
                  </IconButton>
                </Tooltip>
              </Box>
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
          <Box {...getRootProps({ noClick: false })}>
            <input {...getInputProps()} />
            <Button
              variant="contained"
              component="label"
              sx={{ mt: 1, mb: 2 }}
              onClick={openFileDialog}
            >
              Choose or drag an image
            </Button>
          </Box>
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
    </Collapsible>
  );
};
