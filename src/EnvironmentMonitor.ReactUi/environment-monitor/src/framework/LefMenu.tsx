import { Box, Drawer, Typography } from "@mui/material";
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
    <Drawer anchor="left" open={isOpen} variant="persistent">
      <Box
        sx={{
          mt: 10,
          pr: 1,
          pl: 1,
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
          height: "100%",
          maxWidth: "400px",
        }}
        ref={ref}
      >
        <Box>
          <Box sx={{ mb: 2 }}>
            <Typography variant="button" display="block" gutterBottom>
              {title}
            </Typography>
          </Box>
          {children}
        </Box>
        <Box sx={{ cursor: "pointer", mb: 2 }} onClick={onClose}>
          <ArrowBackIcon />
        </Box>
      </Box>
    </Drawer>
  );
};
