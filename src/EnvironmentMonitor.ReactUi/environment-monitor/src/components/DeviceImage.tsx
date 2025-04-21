import { Box, Button, CircularProgress, Typography } from "@mui/material";
import { Device } from "../models/device";
import { useState } from "react";

export interface DeviceImageProps {
  device: Device | undefined;
  title: string;
}

export const DeviceImage: React.FC<DeviceImageProps> = ({ device, title }) => {
  const [isLoadingImage, setIsLoadingImage] = useState(true);
  if (!device) {
    return <></>;
  }

  // setIsLoadingImage(true);
  const imageUrl = `/api/devices/default-image/${device.deviceIdentifier}`;
  return (
    <Box marginTop={2} display={"flex"} flexDirection={"row"}>
      {device.hasDefaultImage ? (
        <Box sx={{ maxHeight: 600, position: "relative" }}>
          {title !== undefined ? (
            <Typography variant="h6" marginBottom={2}>
              {title}
            </Typography>
          ) : null}
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
        <Button variant="contained">Upload default image</Button>
      )}
    </Box>
  );
};
