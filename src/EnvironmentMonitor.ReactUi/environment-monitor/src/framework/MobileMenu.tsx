import { Box, IconButton, Menu, MenuItem } from "@mui/material";
import React from "react";
import { ArrowRight, Menu as MenuIcon } from "@mui/icons-material";
import { useDispatch, useSelector } from "react-redux";
import { routes } from "../utilities/routes";
import { UserMenu } from "./UserMenu";
import { AuthorizedComponent } from "../components/AuthorizedComponent";
import { RoleNames } from "../enums/roleNames";
import {
  getIsLeftMenuOpen,
  toggleLeftMenuOpen,
} from "../reducers/userInterfaceReducer";
import { getLocations } from "../reducers/measurementReducer";
import type { User } from "../models/user";

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
  const locations = useSelector(getLocations);
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClose = () => {
    setAnchorEl(null);
  };

  const [dashboardAnchorEl, setDashboardAnchorEl] =
    React.useState<null | HTMLElement>(null);
  const isDashboardMenuOpen = Boolean(dashboardAnchorEl);

  const handleDashboardOpen = (event: React.MouseEvent<HTMLElement>) => {
    setDashboardAnchorEl(event.currentTarget);
  };

  const handleDashboardClose = () => {
    setDashboardAnchorEl(null);
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
          <AuthorizedComponent requiredRole={RoleNames.User}>
            <MenuItem
              onClick={
                locations.length > 0
                  ? handleDashboardOpen
                  : (e) => {
                      handleClick(routes.dashboard, e);
                    }
              }
              selected={false}
            >
              Dashboard
              {locations.length > 0 ? <ArrowRight sx={{ mr: 1 }} /> : null}
            </MenuItem>
            {locations.length > 0 ? (
              <Menu
                anchorEl={dashboardAnchorEl}
                open={isDashboardMenuOpen}
                onClose={handleDashboardClose}
                anchorOrigin={{ vertical: "top", horizontal: "right" }}
                transformOrigin={{ vertical: "top", horizontal: "left" }}
              >
                <MenuItem
                  onClick={(event) => {
                    handleDashboardClose();
                    handleClick(routes.dashboard, event);
                  }}
                >
                  Devices
                </MenuItem>
                <MenuItem
                  onClick={(event) => {
                    handleDashboardClose();
                    handleClick(routes.locationDashboard, event);
                  }}
                >
                  Locations
                </MenuItem>
              </Menu>
            ) : null}
          </AuthorizedComponent>
          <AuthorizedComponent requiredRole={RoleNames.User}>
            <MenuItem
              selected={false}
              onClick={(event) => {
                handleClick(routes.measurements, event);
              }}
            >
              Measurements
            </MenuItem>
          </AuthorizedComponent>
          <AuthorizedComponent requiredRole={RoleNames.Admin}>
            <MenuItem
              onClick={() => {
                onNavigate(routes.devices);
              }}
            >
              Devices
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
