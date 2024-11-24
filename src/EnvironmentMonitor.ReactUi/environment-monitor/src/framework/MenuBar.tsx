import { AppBar, useMediaQuery, useTheme } from "@mui/material";
import React from "react";

import { DesktopMenu } from "./DesktopMenu";
import { MobileMenu } from "./MobileMenu";
import { User } from "../models/user";

export interface MenuBarProps {
  handleLogOut: () => void;
  handleNavigateTo: (route: string) => void;
  user: User | undefined;
}

export const MenuBar: React.FC<MenuBarProps> = ({
  handleLogOut,
  handleNavigateTo,
  user,
}) => {
  const theme = useTheme();
  const drawDesktop = useMediaQuery(theme.breakpoints.up("lg"));

  return (
    <AppBar
      position="fixed"
      sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}
    >
      {!drawDesktop ? (
        <MobileMenu
          onNavigate={handleNavigateTo}
          onLogin={() => {
            //
          }}
          onLogOut={handleLogOut}
          user={user}
        />
      ) : (
        <DesktopMenu
          onNavigate={handleNavigateTo}
          onLogin={() => {
            //
          }}
          onLogOut={handleLogOut}
          user={user}
        />
      )}
    </AppBar>
  );
};
