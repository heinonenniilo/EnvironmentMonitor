import { Box, Container, IconButton, MenuItem } from "@mui/material";
import React from "react";
import { useDispatch, useSelector } from "react-redux";
import styled from "styled-components";
import { Menu as MenuIcon } from "@mui/icons-material";
import { routes } from "../utilities/routes";
import { UserMenu } from "./UserMenu";
import { User } from "../models/user";
import { getIsLoggedIn } from "../reducers/userReducer";

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
  const isLeftMenuOpen = false; //useSelector(getLeftMenuIsOpen);
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
              // dispatch(appUiActions.toggleLeftMenu(!isLeftMenuOpen));
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

          <MenuItem
            onClick={() => {
              onNavigate(routes.measurements);
            }}
          >
            Measurements
          </MenuItem>
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
