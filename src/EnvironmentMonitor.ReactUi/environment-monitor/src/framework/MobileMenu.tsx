import {
  Box,
  IconButton,
  Menu,
  MenuItem,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";
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
import logo from "../assets/logo.png";

export interface MobileMenuProps {
  onNavigate: (route: string) => void;
  onLogin: () => void;
  onLogOut: () => void;
  user: User | undefined;
}

export const MobileMenu: React.FC<MobileMenuProps> = ({
  onNavigate,
  onLogOut,
  onLogin,
  user,
}) => {
  const dispath = useDispatch();
  const isLeftMenuOpen = useSelector(getIsLeftMenuOpen);
  const locations = useSelector(getLocations);
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const theme = useTheme();
  const drawUserInMenu = useMediaQuery(theme.breakpoints.up("sm"));
  const [manageAchor, setManageAnchor] = React.useState<null | HTMLElement>(
    null
  );
  const open = Boolean(anchorEl);
  const manageOpen = Boolean(manageAchor);
  const handleClose = () => {
    setAnchorEl(null);
  };

  const [dashboardAnchorEl, setDashboardAnchorEl] =
    React.useState<null | HTMLElement>(null);
  const isDashboardMenuOpen = Boolean(dashboardAnchorEl);

  const [measurementsAnchorEl, setMeasurementsAnchorEl] =
    React.useState<null | HTMLElement>(null);
  const isMeasurementsMenuOpen = Boolean(measurementsAnchorEl);

  const handleDashboardOpen = (event: React.MouseEvent<HTMLElement>) => {
    setDashboardAnchorEl(event.currentTarget);
  };

  const handleDashboardClose = () => {
    setDashboardAnchorEl(null);
  };

  const handleMeasurementsOpen = (event: React.MouseEvent<HTMLElement>) => {
    setMeasurementsAnchorEl(event.currentTarget);
  };

  const handleMeasurementsClose = () => {
    setMeasurementsAnchorEl(null);
  };

  const handleClick = (
    route: string,
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    _event: React.MouseEvent<HTMLLIElement, MouseEvent>
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
              handleClick(routes.home, event);
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
              onClick={
                locations.length > 0
                  ? handleMeasurementsOpen
                  : (e) => {
                      handleClick(routes.measurements, e);
                    }
              }
              selected={false}
            >
              Measurements
              {locations.length > 0 ? <ArrowRight sx={{ mr: 1 }} /> : null}
            </MenuItem>
            {locations.length > 0 ? (
              <Menu
                anchorEl={measurementsAnchorEl}
                open={isMeasurementsMenuOpen}
                onClose={handleMeasurementsClose}
                anchorOrigin={{ vertical: "top", horizontal: "right" }}
                transformOrigin={{ vertical: "top", horizontal: "left" }}
              >
                <MenuItem
                  onClick={(event) => {
                    handleMeasurementsClose();
                    handleClick(routes.measurements, event);
                  }}
                >
                  Devices
                </MenuItem>
                <MenuItem
                  onClick={(event) => {
                    handleMeasurementsClose();
                    handleClick(routes.locationMeasurements, event);
                  }}
                >
                  Locations
                </MenuItem>
              </Menu>
            ) : null}
          </AuthorizedComponent>
          <Menu
            anchorEl={manageAchor}
            open={manageOpen}
            onClose={() => {
              setManageAnchor(null);
            }}
            anchorOrigin={{ vertical: "top", horizontal: "right" }}
            transformOrigin={{ vertical: "top", horizontal: "left" }}
          >
            <MenuItem
              onClick={(event) => {
                setManageAnchor(null);
                handleClick(routes.devices, event);
              }}
            >
              Devices
            </MenuItem>
            <MenuItem
              onClick={(event) => {
                setManageAnchor(null);
                handleClick(routes.deviceMessages, event);
              }}
            >
              Device messages
            </MenuItem>
            <MenuItem
              onClick={(event) => {
                setManageAnchor(null);
                handleClick(routes.deviceEmails, event);
              }}
            >
              Email templates
            </MenuItem>
            <MenuItem
              onClick={(event) => {
                setManageAnchor(null);
                handleClick(routes.users, event);
              }}
            >
              Users
            </MenuItem>
          </Menu>
          <AuthorizedComponent requiredRole={RoleNames.Admin}>
            <MenuItem
              onClick={(event) => {
                setManageAnchor(event.currentTarget);
              }}
            >
              Manage <ArrowRight sx={{ mr: 1 }} />
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
        <IconButton>
          <Box
            component="img"
            src={logo}
            alt="Logo"
            sx={{
              height: 30,
              cursor: "pointer",
              display: "flex",
            }}
            onClick={() => onNavigate(routes.main)}
          />
        </IconButton>
        <Box sx={{ marginTop: "auto", marginBottom: "auto", mr: 2 }}>
          <Typography color="text.secondary" variant="subtitle2">
            Environment Monitor
          </Typography>
        </Box>
        {drawMenu()}
      </Box>

      <Box sx={{ marginLeft: "auto" }}>
        <UserMenu
          user={user}
          handleLogOut={onLogOut}
          isMobile
          drawUserInMenu={drawUserInMenu}
          handleLogIn={onLogin}
        />
      </Box>
    </Box>
  );
};
