import { Box, CircularProgress } from "@mui/material";

export interface LoadingOverlayProps {
  isLoading: boolean;
}

export const LoadingOverlay: React.FC<LoadingOverlayProps> = ({
  isLoading,
}) => {
  if (!isLoading) {
    return null;
  }

  return (
    <Box
      position="absolute"
      top={0}
      left={0}
      width="100%"
      height="100%"
      display="flex"
      justifyContent="center"
      alignItems="center"
      sx={{
        backgroundColor: "rgba(255,255,255,0.5)",
        zIndex: 2,
      }}
    >
      <CircularProgress />
    </Box>
  );
};
