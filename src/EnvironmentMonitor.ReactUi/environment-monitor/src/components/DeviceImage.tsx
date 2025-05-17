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
  WidthFull,
} from "@mui/icons-material";
import { type DeviceInfo } from "../models/deviceInfo";
import { Collapsible } from "./CollabsibleComponent";
import { useSwipeable } from "react-swipeable";
import { useDropzone } from "react-dropzone";
import { DeviceImageDialog } from "./DeviceImageDialog";
import { dateTimeSort, getFormattedDate } from "../utilities/datetimeUtils";
import { formatBytes } from "../utilities/stringUtils";

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
  onUploadImage,
  onDeleteImage,
  onSetDefaultImage,
}) => {
  const [isLoadingImage, setIsLoadingImage] = useState(true);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string>("");

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
    if (attachments.length <= 1) {
      return;
    }
    setCurrentIndex(
      (prev) => (prev - 1 + attachments.length) % attachments.length
    );
    setIsLoadingImage(true);
  };

  const nextImage = () => {
    if (attachments.length <= 1) {
      return;
    }
    setCurrentIndex((prev) => (prev + 1) % attachments.length);
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

  const attachments = device?.attachments
    ? device?.attachments.sort((a, b) => dateTimeSort(b.created, a.created))
    : [];

  const activeAttachment =
    attachments.length > currentIndex ? attachments[currentIndex] : undefined;

  const isDefaultImage = () => {
    if (device === undefined || activeAttachment === undefined) {
      return false;
    }
    return device.defaultImageGuid === activeAttachment.guid;
  };

  const getCurrentAttachmentUrl = () => {
    if (!activeAttachment) {
      return "";
    }

    return `/api/devices/attachment/${device?.device.identifier}/${activeAttachment.guid}`;
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
      <DeviceImageDialog
        isOpen={imagePreviewUrl.length > 0}
        imageUrl={imagePreviewUrl}
        onClose={() => {
          setImagePreviewUrl("");
        }}
      />
      <Box
        marginTop={2}
        display={"flex"}
        flexDirection={"row"}
        {...getRootProps()}
      >
        <input {...getInputProps()} />
        {attachments !== undefined && attachments.length > 0 ? (
          <Box
            sx={{
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
            <Box
              component={"img"}
              src={getCurrentAttachmentUrl()}
              alt="Device"
              style={{
                width: "100%",
                display: "block",
                height: "auto",
                minHeight: "300px",
                maxHeight: "min(80vh, 600px)",
                objectFit: "contain",
                filter: isLoadingImage ? "blur(10px)" : "none",
                transition: "filter 0.3s ease-in-out",
              }}
              sx={{ order: { xs: 2, sm: 1 } }}
              onLoad={() => setIsLoadingImage(false)}
              onError={() => {
                setIsLoadingImage(false);
              }}
            />
            <Box
              sx={{
                display: "flex",
                flexDirection: { xs: "column", sm: "row" },
                order: { xs: 1, sm: 2 },
              }}
            >
              <Typography variant="body2" fontWeight="bold" mr={1}>
                Added:
              </Typography>
              <Typography variant="body2">
                {activeAttachment !== undefined
                  ? getFormattedDate(activeAttachment?.created)
                  : "-"}
              </Typography>
              <Typography
                variant="body2"
                fontWeight="bold"
                sx={{ ml: { xs: 0, sm: 1 } }}
                mr={1}
              >
                Name:
              </Typography>
              <Typography variant="body2">{activeAttachment?.name}</Typography>
              <Typography
                variant="body2"
                fontWeight="bold"
                sx={{ ml: { xs: 0, sm: 1 } }}
                mr={1}
              >
                Size:
              </Typography>
              <Typography variant="body2">
                {activeAttachment && activeAttachment.sizeInBytes
                  ? formatBytes(activeAttachment.sizeInBytes)
                  : ""}
              </Typography>
            </Box>
            <Box
              sx={{
                display: "flex",
                flexDirection: "row",
                justifyContent: "space-between",
                alignItems: "center",
                order: 3,
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
                <Typography variant="caption" textAlign={"center"}>{` ${
                  currentIndex + 1
                }/${attachments.length}`}</Typography>
                <Tooltip title={"Delete image?"}>
                  <IconButton
                    onClick={() => {
                      if (!activeAttachment) {
                        return;
                      }
                      onDeleteImage(activeAttachment.guid);
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
                      if (!activeAttachment) {
                        return;
                      }
                      onSetDefaultImage(activeAttachment.guid);
                    }}
                    disabled={isDefaultImage()}
                    sx={{ ml: 1, cursor: "pointer" }}
                    size="small"
                  >
                    <CheckCircle />
                  </IconButton>
                </Tooltip>
                <Tooltip title={"Expand"}>
                  <IconButton
                    onClick={() => {
                      setImagePreviewUrl(getCurrentAttachmentUrl());
                    }}
                    sx={{ ml: 1, cursor: "pointer" }}
                    size="small"
                  >
                    <WidthFull />
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
