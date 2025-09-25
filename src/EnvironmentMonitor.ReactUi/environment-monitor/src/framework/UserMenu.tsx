import { AccountCircle } from "@mui/icons-material";
import {
  Box,
  Button,
  IconButton,
  Menu,
  MenuItem,
  Typography,
} from "@mui/material";
import React from "react";
import type { User } from "../models/user";

export interface UserMenuProps {
  handleLogOut: () => void;
  handleLogIn: () => void;
  user: User | undefined;
  isMobile?: boolean;
  drawUserInMenu?: boolean;
}

export const UserMenu: React.FC<UserMenuProps> = ({
  user,
  handleLogOut,
  isMobile,
  drawUserInMenu,
  handleLogIn,
}) => {
  const [userMenuAcnhor, setUserMenuAcnhor] =
    React.useState<null | HTMLElement>(null);

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setUserMenuAcnhor(event.currentTarget);
  };

  const handleClose = () => {
    setUserMenuAcnhor(null);
  };

  if (user) {
    return (
      <Box sx={{ display: "flex", flexDirection: "row" }}>
        {drawUserInMenu && (
          <Typography
            variant={!isMobile ? "h6" : "body1"}
            marginTop={"auto"}
            marginBottom={"auto"}
          >
            {user.email}
          </Typography>
        )}

        <IconButton
          size="large"
          aria-label="account of current user"
          aria-controls="menu-appbar"
          aria-haspopup="true"
          color="inherit"
          onClick={handleMenu}
        >
          <AccountCircle />
        </IconButton>
        <Menu
          id="menu-appbar"
          anchorEl={userMenuAcnhor}
          anchorOrigin={{
            vertical: "top",
            horizontal: "right",
          }}
          keepMounted
          transformOrigin={{
            vertical: "top",
            horizontal: "right",
          }}
          open={Boolean(userMenuAcnhor)}
          onClose={handleClose}
        >
          {!drawUserInMenu && (
            <Box sx={{ alignItems: "center", margin: 2 }}>
              <Typography variant="subtitle2" color="text.secondary">
                {user.email}
              </Typography>
            </Box>
          )}

          <MenuItem>
            <Button
              color="inherit"
              onClick={() => {
                handleClose();
                handleLogOut();
              }}
            >
              Log out
            </Button>
          </MenuItem>
        </Menu>
      </Box>
    );
  } else {
    return (
      <MenuItem>
        <Button
          variant="contained"
          onClick={() => {
            handleLogIn();
          }}
        >
          Login
        </Button>
      </MenuItem>
    );
  }
};
