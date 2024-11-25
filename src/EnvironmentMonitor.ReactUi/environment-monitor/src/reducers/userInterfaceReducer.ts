import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import { RootState } from "../setup/appStore";

export interface UserInterfaceState {
  leftMenuOpen: boolean;
  hasLeftMenu: boolean;
}

const initialState: UserInterfaceState = {
  leftMenuOpen: false,
  hasLeftMenu: false,
};

export const userInterfaceSlice = createSlice({
  name: "userInterface",
  initialState,
  reducers: {
    toggleLeftMenuOpen: (state, action: PayloadAction<boolean>) => {
      state.leftMenuOpen = action.payload;
    },
    toggleHasLeftMenu: (state, action: PayloadAction<boolean>) => {
      state.hasLeftMenu = action.payload;
    },
  },
});

export const { toggleLeftMenuOpen, toggleHasLeftMenu } =
  userInterfaceSlice.actions;

export const getIsLeftMenuOpen = (state: RootState): boolean =>
  state.userInterfaceInfo.leftMenuOpen;

export const getHasLeftMenu = (state: RootState): boolean =>
  state.userInterfaceInfo.hasLeftMenu;

export default userInterfaceSlice.reducer;
