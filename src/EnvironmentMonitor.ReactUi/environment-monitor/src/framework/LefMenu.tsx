import { Box, Drawer, IconButton, Typography } from "@mui/material";
import React, { useEffect, useRef, useState, type JSX } from "react";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";

export interface LeftMenuProps {
  title: string;
  children: JSX.Element;
  isOpen: boolean;
  setMenuWidth?: (width: number) => void;
  onClose: () => void;
}

export const LeftMenu: React.FC<LeftMenuProps> = ({
  title,
  children,
  setMenuWidth,
  isOpen,
  onClose,
}) => {
  const ref = useRef<HTMLElement | undefined | null>(null);

  const [menuWidth, setWidth] = useState<number | undefined>(undefined);

  useEffect(() => {
    if (ref?.current?.offsetWidth) {
      setWidth(ref?.current.offsetWidth);
    }
  }, []);

  useEffect(() => {
    if (setMenuWidth && menuWidth) {
      setMenuWidth(menuWidth);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [menuWidth]);

  return (
    <Drawer
      anchor="left"
      open={isOpen}
      variant="persistent"
      sx={{
        "& .MuiDrawer-paper": {
          overflow: "hidden",
          height: "100dvh",
        },
      }}
    >
      <Box
        sx={{
          pt: 10,
          pr: 1,
          pl: 1,
          display: "flex",
          flexDirection: "column",
          height: "100%",
          minHeight: 0,
          boxSizing: "border-box",
          maxWidth: "400px",
        }}
        ref={ref}
      >
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            gap: 1,
            flexShrink: 0,
            pb: 1,
          }}
        >
          <IconButton aria-label="Close filters" onClick={onClose}>
            <ArrowBackIcon />
          </IconButton>
          <Typography variant="button" display="block">
            {title}
          </Typography>
        </Box>
        <Box sx={{ flex: 1, minHeight: 0, overflowY: "auto" }}>
          {children}
        </Box>
      </Box>
    </Drawer>
  );
};
