import { NotificationMessage } from "./../framework/NotificationsComponent";
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
  notifications: NotificationMessage[];
}

const initialState: UserInterfaceState = {
  leftMenuOpen: true,
  hasLeftMenu: false,
  confirmationDialog: undefined,
  notifications: [],
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
    addNotification: (state, action: PayloadAction<NotificationMessage>) => {
      const count = state.notifications.length;
      let messageToAdd = action.payload;
      messageToAdd.id = count;
      state.notifications.push(messageToAdd);
    },
    removeNotification: (state, action: PayloadAction<number | undefined>) => {
      if (action.payload === undefined) {
        state.notifications = [];
      } else {
        const id = state.notifications.findIndex(
          (x) => x.id === action.payload
        );
        if (id !== -1) {
          state.notifications.splice(id, 1);
        }
      }
    },
  },
});

export const {
  toggleLeftMenuOpen,
  toggleHasLeftMenu,
  setConfirmDialog,
  addNotification,
  removeNotification,
} = userInterfaceSlice.actions;

export const getIsLeftMenuOpen = (state: RootState): boolean =>
  state.userInterfaceInfo.leftMenuOpen;

export const getHasLeftMenu = (state: RootState): boolean =>
  state.userInterfaceInfo.hasLeftMenu;

export const getConfirmationDialog = (
  state: RootState
): SetConfirmationDialogActionPayload | undefined =>
  state.userInterfaceInfo.confirmationDialog;

export const getNotifications = (state: RootState): NotificationMessage[] =>
  state.userInterfaceInfo.notifications;

export default userInterfaceSlice.reducer;
