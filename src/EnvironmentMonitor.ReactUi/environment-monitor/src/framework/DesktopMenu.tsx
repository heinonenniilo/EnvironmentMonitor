import {
  Box,
  Container,
  IconButton,
  Menu,
  MenuItem,
  Typography,
} from "@mui/material";
import React from "react";
import { useDispatch, useSelector } from "react-redux";
import styled from "styled-components";
import { Menu as MenuIcon } from "@mui/icons-material";
import { routes } from "../utilities/routes";
import { UserMenu } from "./UserMenu";
import { AuthorizedComponent } from "../components/AuthorizedComponent";
import { RoleNames } from "../enums/roleNames";
import {
  getIsLeftMenuOpen,
  toggleLeftMenuOpen,
} from "../reducers/userInterfaceReducer";
import { ArrowDropDownIcon } from "@mui/x-date-pickers";
import { getLocations } from "../reducers/measurementReducer";
import type { User } from "../models/user";
import logo from "../assets/logo.png";

export interface DesktopMenuProps {
  onNavigate: (route: string) => void;
  onLogin: () => void;
  onLogOut: () => void;
  user: User | undefined;
}

const MenuItemsContainer = styled.div`
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  width: 100%;
  max-width: 100%;
`;

const MenuArea = styled.div`
  display: flex;
  flex-direction: row;
`;

export const DesktopMenu: React.FC<DesktopMenuProps> = ({
  onNavigate,
  onLogOut,
  onLogin,
  user,
}) => {
  const isLeftMenuOpen = useSelector(getIsLeftMenuOpen);
  const dispath = useDispatch();
  const locations = useSelector(getLocations);

  // const user = useSelector(getUserInfo);

  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const [anchorE2, setAnchorE2] = React.useState<null | HTMLElement>(null);
  const [measurementsAnchorEl, setMeasurementsAnchorEl] =
    React.useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const manageMenuOpen = Boolean(anchorE2);
  const measurementsMenuOpen = Boolean(measurementsAnchorEl);

  const handleDashboardClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleMeasurementsClick = (event: React.MouseEvent<HTMLElement>) => {
    setMeasurementsAnchorEl(event.currentTarget);
  };

  const handleMeasurementsMenuClose = () => {
    setMeasurementsAnchorEl(null);
  };

  return (
    <Container maxWidth={"xl"}>
      <MenuItemsContainer>
        <MenuArea>
          <IconButton
            size="large"
            edge="start"
            color="inherit"
            aria-label="menu"
            onClick={() => {
              dispath(toggleLeftMenuOpen(!isLeftMenuOpen));
            }}
          >
            <MenuIcon />
          </IconButton>
          <MenuItem sx={{ pl: 1, pr: 1 }}>
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
          </MenuItem>
          <Box
            sx={{
              alignItems: "center",
              marginTop: "auto",
              marginBottom: "auto",
              marginRight: 2,
            }}
          >
            <Typography variant="subtitle1" color="text.secondary">
              Environment Monitor
            </Typography>
          </Box>
          <AuthorizedComponent requiredRole={RoleNames.User}>
            <MenuItem
              onClick={() => {
                onNavigate(routes.home);
              }}
            >
              Home
            </MenuItem>
          </AuthorizedComponent>
          <AuthorizedComponent requiredRole={RoleNames.User}>
            {locations.length > 0 ? (
              <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleMenuClose}
                anchorOrigin={{
                  vertical: "bottom",
                  horizontal: "left",
                }}
                transformOrigin={{
                  vertical: "top",
                  horizontal: "left",
                }}
              >
                <MenuItem
                  onClick={() => {
                    handleMenuClose();
                    onNavigate(routes.dashboard);
                  }}
                >
                  Devices
                </MenuItem>
                <MenuItem
                  onClick={() => {
                    handleMenuClose();
                    onNavigate(routes.locationDashboard);
                  }}
                >
                  Locations
                </MenuItem>
              </Menu>
            ) : null}
            <MenuItem
              onClick={
                locations.length > 0
                  ? handleDashboardClick
                  : () => {
                      onNavigate(routes.dashboard);
                    }
              }
              sx={{ display: "flex", alignItems: "center", gap: 0.5 }}
            >
              Dashboard
              {locations.length > 0 ? <ArrowDropDownIcon /> : null}
            </MenuItem>
          </AuthorizedComponent>
          <AuthorizedComponent requiredRole={RoleNames.User}>
            {locations.length > 0 ? (
              <Menu
                anchorEl={measurementsAnchorEl}
                open={measurementsMenuOpen}
                onClose={handleMeasurementsMenuClose}
                anchorOrigin={{
                  vertical: "bottom",
                  horizontal: "left",
                }}
                transformOrigin={{
                  vertical: "top",
                  horizontal: "left",
                }}
              >
                <MenuItem
                  onClick={() => {
                    handleMeasurementsMenuClose();
                    onNavigate(routes.measurements);
                  }}
                >
                  Devices
                </MenuItem>
                <MenuItem
                  onClick={() => {
                    handleMeasurementsMenuClose();
                    onNavigate(routes.locationMeasurements);
                  }}
                >
                  Locations
                </MenuItem>
              </Menu>
            ) : null}
            <MenuItem
              onClick={
                locations.length > 0
                  ? handleMeasurementsClick
                  : () => {
                      onNavigate(routes.measurements);
                    }
              }
              sx={{ display: "flex", alignItems: "center", gap: 0.5 }}
            >
              Measurements
              {locations.length > 0 ? <ArrowDropDownIcon /> : null}
            </MenuItem>
          </AuthorizedComponent>
          <AuthorizedComponent requiredRole={RoleNames.Admin}>
            <Menu
              anchorEl={anchorE2}
              open={manageMenuOpen}
              onClose={() => {
                setAnchorE2(null);
              }}
              anchorOrigin={{
                vertical: "bottom",
                horizontal: "left",
              }}
              transformOrigin={{
                vertical: "top",
                horizontal: "left",
              }}
            >
              <MenuItem
                onClick={() => {
                  handleMenuClose();
                  setAnchorE2(null);
                  onNavigate(routes.devices);
                }}
              >
                Devices
              </MenuItem>
              <MenuItem
                onClick={() => {
                  handleMenuClose();
                  setAnchorE2(null);
                  onNavigate(routes.deviceMessages);
                }}
              >
                Device messages
              </MenuItem>
              <MenuItem
                onClick={() => {
                  handleMenuClose();
                  setAnchorE2(null);
                  onNavigate(routes.deviceEmails);
                }}
              >
                Email templates
              </MenuItem>
            </Menu>
            <MenuItem
              onClick={(event) => {
                setAnchorE2(event.currentTarget);
              }}
            >
              Manage
              <ArrowDropDownIcon />
            </MenuItem>
          </AuthorizedComponent>
        </MenuArea>
        <Box>
          <MenuArea>
            <UserMenu
              user={user}
              handleLogOut={onLogOut}
              drawUserInMenu
              handleLogIn={onLogin}
              showProviderIcon
            />
          </MenuArea>
        </Box>
      </MenuItemsContainer>
    </Container>
  );
};
