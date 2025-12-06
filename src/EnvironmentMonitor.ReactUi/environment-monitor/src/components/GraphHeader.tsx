import {
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  IconButton,
  Typography,
} from "@mui/material";
import { Close, Fullscreen } from "@mui/icons-material";
import { Link } from "react-router";
import { type ReactNode } from "react";

export interface GraphHeaderProps {
  title: string;
  titleAsLink?: boolean;
  linkTo?: string;
  zoomable?: boolean;
  hideUseAutoScale?: boolean;
  autoScale?: boolean;
  onResetZoom?: () => void;
  onFullScreen?: () => void;
  onClose?: () => void;
  onSetAutoScale?: (state: boolean) => void;
  onRefresh?: () => void;
  renderControls?: () => ReactNode;
  showControls?: boolean;
}

export const GraphHeader: React.FC<GraphHeaderProps> = ({
  title,
  titleAsLink,
  linkTo,
  zoomable,
  hideUseAutoScale,
  autoScale,
  onResetZoom,
  onFullScreen,
  onClose,
  onSetAutoScale,
  onRefresh,
  renderControls,
  showControls,
}) => {
  const renderDefaultControls = () => (
    <>
      {!hideUseAutoScale && (
        <FormControlLabel
          control={
            <Checkbox
              checked={autoScale ?? false}
              onChange={(_e, c) => {
                if (onSetAutoScale) {
                  onSetAutoScale(c);
                }
              }}
              inputProps={{ "aria-label": "auto scale checkbox" }}
            />
          }
          label="Auto scale"
          componentsProps={{
            typography: { fontSize: "14px" },
          }}
        />
      )}
      {onRefresh && (
        <Button variant="outlined" onClick={onRefresh} size="small">
          Refresh
        </Button>
      )}
    </>
  );

  return (
    <Box
      width="100%"
      mt={0}
      flexGrow={0}
      flexDirection="row"
      display="flex"
      alignItems="center"
    >
      {titleAsLink && linkTo ? (
        <Link to={linkTo}>
          <Typography align="left" gutterBottom>
            {title}
          </Typography>
        </Link>
      ) : (
        <Typography align="left" gutterBottom>
          {title}
        </Typography>
      )}
      {showControls ? (
        <Box
          sx={{
            display: "flex",
            flexDirection: "row",
            marginLeft: "auto",
            gap: 1,
            alignItems: "center",
          }}
        >
          {renderControls ? renderControls() : renderDefaultControls()}
          {zoomable && onResetZoom && (
            <Button variant="outlined" onClick={onResetZoom} size="small">
              Reset zoom
            </Button>
          )}
          {onFullScreen && (
            <IconButton
              onClick={onFullScreen}
              size="small"
              aria-label="fullscreen"
            >
              <Fullscreen />
            </IconButton>
          )}
          {onClose && (
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
          )}
        </Box>
      ) : null}
    </Box>
  );
};
