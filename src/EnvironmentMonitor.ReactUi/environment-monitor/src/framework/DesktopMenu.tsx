import { Box, Container, IconButton, MenuItem } from "@mui/material";
import React from "react";
import { useDispatch, useSelector } from "react-redux";
import styled from "styled-components";
import { Menu as MenuIcon } from "@mui/icons-material";
import { routes } from "../utilities/routes";
import { UserMenu } from "./UserMenu";
import { User } from "../models/user";
import { AuthorizedComponent } from "../components/AuthorizedComponent";
import { RoleNames } from "../enums/roleNames";
import {
  getIsLeftMenuOpen,
  toggleLeftMenuOpen,
} from "../reducers/userInterfaceReducer";

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
  onLogin,
  onLogOut,
  user,
}) => {
  const isLeftMenuOpen = useSelector(getIsLeftMenuOpen);
  const dispath = useDispatch();
  // const user = useSelector(getUserInfo);

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
          <AuthorizedComponent requiredRole={RoleNames.Viewer}>
            <MenuItem
              onClick={() => {
                onNavigate(routes.dashboard);
              }}
            >
              Dashboard
            </MenuItem>
          </AuthorizedComponent>
          <AuthorizedComponent requiredRole={RoleNames.Viewer}>
            <MenuItem
              onClick={() => {
                onNavigate(routes.measurements);
              }}
            >
              Measurements
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
