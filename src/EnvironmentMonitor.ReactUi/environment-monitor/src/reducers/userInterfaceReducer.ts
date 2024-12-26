import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import { RootState } from "../setup/appStore";

export interface SetConfirmationDialogActionPayload {
  onConfirm: () => void;
  title: string;
  body: string;
}

export interface UserInterfaceState {
  leftMenuOpen: boolean;
  hasLeftMenu: boolean;
  confirmationDialog: SetConfirmationDialogActionPayload | undefined;
}

const initialState: UserInterfaceState = {
  leftMenuOpen: true,
  hasLeftMenu: false,
  confirmationDialog: undefined,
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
    setConfirmDialog: (
      state,
      action: PayloadAction<SetConfirmationDialogActionPayload | undefined>
    ) => {
      state.confirmationDialog = action.payload;
    },
  },
});

export const { toggleLeftMenuOpen, toggleHasLeftMenu, setConfirmDialog } =
  userInterfaceSlice.actions;

export const getIsLeftMenuOpen = (state: RootState): boolean =>
  state.userInterfaceInfo.leftMenuOpen;

export const getHasLeftMenu = (state: RootState): boolean =>
  state.userInterfaceInfo.hasLeftMenu;

export const getConfirmationDialog = (
  state: RootState
): SetConfirmationDialogActionPayload | undefined =>
  state.userInterfaceInfo.confirmationDialog;

export default userInterfaceSlice.reducer;
