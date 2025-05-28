import { Box, Container, IconButton, Menu, MenuItem } from "@mui/material";
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
  user,
}) => {
  const isLeftMenuOpen = useSelector(getIsLeftMenuOpen);
  const dispath = useDispatch();
  const locations = useSelector(getLocations);

  // const user = useSelector(getUserInfo);

  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const [anchorE2, setAnchorE2] = React.useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const manageMenuOpen = Boolean(anchorE2);

  const handleDashboardClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
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
            sx={{ mr: 2 }}
            onClick={() => {
              dispath(toggleLeftMenuOpen(!isLeftMenuOpen));
            }}
          >
            <MenuIcon />
          </IconButton>
          <MenuItem
            onClick={() => {
              onNavigate(routes.main);
            }}
          >
            Home
          </MenuItem>
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
            <MenuItem
              onClick={() => {
                onNavigate(routes.measurements);
              }}
            >
              Measurements
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
            <UserMenu user={user} handleLogOut={onLogOut} />
          </MenuArea>
        </Box>
      </MenuItemsContainer>
    </Container>
  );
};
