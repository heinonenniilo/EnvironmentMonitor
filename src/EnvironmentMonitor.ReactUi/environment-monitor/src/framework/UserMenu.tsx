import { AccountCircle, Google, Microsoft, GitHub } from "@mui/icons-material";
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
import { useNavigate } from "react-router";
import { routes } from "../utilities/routes";
import { RoleNames } from "../enums/roleNames";

export interface UserMenuProps {
  handleLogOut: () => void;
  handleLogIn: () => void;
  user: User | undefined;
  isMobile?: boolean;
  drawUserInMenu?: boolean;
  showProviderIcon?: boolean;
}

export const UserMenu: React.FC<UserMenuProps> = ({
  user,
  handleLogOut,
  isMobile,
  drawUserInMenu,
  handleLogIn,
  showProviderIcon,
}) => {
  const [userMenuAcnhor, setUserMenuAcnhor] =
    React.useState<null | HTMLElement>(null);
  const navigate = useNavigate();

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setUserMenuAcnhor(event.currentTarget);
  };

  const handleClose = () => {
    setUserMenuAcnhor(null);
  };

  const canSeeOwnInfo = user?.roles?.some(
    (r) =>
      r === RoleNames.Admin ||
      r === RoleNames.User ||
      r === RoleNames.Registered
  );

  const getProviderIcon = () => {
    if (!showProviderIcon || !user?.authenticationProvider) return null;

    const baseIconProps = {
      fontSize: 24,
      mr: 0.5,
      verticalAlign: "middle",
      color: "#000000",
    };

    const provider = user.authenticationProvider.toLowerCase();

    if (provider === "google") {
      return <Google sx={{ ...baseIconProps }} />;
    }
    if (provider === "microsoft") {
      return <Microsoft sx={{ ...baseIconProps }} />;
    }
    if (provider === "github") {
      return <GitHub sx={{ ...baseIconProps }} />;
    }
    return null;
  };

  if (user) {
    return (
      <Box sx={{ display: "flex", flexDirection: "row" }}>
        {drawUserInMenu && (
          <Typography
            variant={!isMobile ? "h6" : "body1"}
            marginTop={"auto"}
            marginBottom={"auto"}
            sx={{ display: "flex", alignItems: "center" }}
          >
            {getProviderIcon()}
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
              <Typography
                variant="subtitle2"
                color="text.secondary"
                sx={{ display: "flex", alignItems: "center" }}
              >
                {getProviderIcon()}
                {user.email}
              </Typography>
            </Box>
          )}
          {canSeeOwnInfo && (
            <MenuItem>
              <Button
                color="inherit"
                onClick={() => {
                  handleClose();
                  navigate(routes.userInfo);
                }}
              >
                User Info
              </Button>
            </MenuItem>
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
