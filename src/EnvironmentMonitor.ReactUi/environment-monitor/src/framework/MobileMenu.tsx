import { Box, IconButton, Menu, MenuItem } from "@mui/material";
import React from "react";
import { Menu as MenuIcon } from "@mui/icons-material";
import { useDispatch, useSelector } from "react-redux";
import { User } from "../models/user";
import { routes } from "../utilities/routes";
import { UserMenu } from "./UserMenu";
import { AuthorizedComponent } from "../components/AuthorizedComponent";
import { RoleNames } from "../enums/roleNames";
import {
  getIsLeftMenuOpen,
  toggleLeftMenuOpen,
} from "../reducers/userInterfaceReducer";

export interface MobileMenuProps {
  onNavigate: (route: string) => void;
  onLogin: () => void;
  onLogOut: () => void;
  user: User | undefined;
}

export const MobileMenu: React.FC<MobileMenuProps> = ({
  onNavigate,
  onLogin,
  onLogOut,
  user,
}) => {
  const dispath = useDispatch();
  const isLeftMenuOpen = useSelector(getIsLeftMenuOpen);
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleClick = (
    route: string,
    event: React.MouseEvent<HTMLLIElement, MouseEvent>
  ) => {
    setAnchorEl(null);
    onNavigate(route);
  };

  const drawMenu = () => {
    if (!user) {
      return;
    }

    if (user) {
      return (
        <Menu
          id="lock-menu"
          anchorEl={anchorEl}
          open={open}
          onClose={handleClose}
          MenuListProps={{
            "aria-labelledby": "lock-button",
            role: "listbox",
          }}
        >
          <MenuItem
            selected={false}
            onClick={(event) => {
              handleClick(routes.main, event);
            }}
          >
            Home
          </MenuItem>
          <AuthorizedComponent requiredRole={RoleNames.Viewer}>
            <MenuItem
              selected={false}
              onClick={(event) => {
                handleClick(routes.dashboard, event);
              }}
            >
              Dashboard
            </MenuItem>
          </AuthorizedComponent>
          <AuthorizedComponent requiredRole={RoleNames.Viewer}>
            <MenuItem
              selected={false}
              onClick={(event) => {
                handleClick(routes.measurements, event);
              }}
            >
              Measurements
            </MenuItem>
          </AuthorizedComponent>
          <MenuItem
            selected={false}
            onClick={() => {
              dispath(toggleLeftMenuOpen(!isLeftMenuOpen));
              handleClose();
            }}
          >
            {isLeftMenuOpen ? "Hide filters" : "Show filters"}
          </MenuItem>
        </Menu>
      );
    }
  };

  // TODO: Check the definition for paddingLeft
  return (
    <Box sx={{ width: "100%", display: "flex", flexDirection: "row" }}>
      <Box sx={{ display: "flex", flexDirection: "row", pl: 1.5 }}>
        <IconButton
          size="large"
          edge="start"
          color="inherit"
          aria-label="menu"
          id="mobileHamburger"
          onClick={(event) => {
            setAnchorEl(event.currentTarget);
          }}
        >
          <MenuIcon />
        </IconButton>
        {drawMenu()}
      </Box>

      <Box sx={{ marginLeft: "auto" }}>
        <UserMenu user={user} handleLogOut={onLogOut} isMobile />
      </Box>
    </Box>
  );
};
