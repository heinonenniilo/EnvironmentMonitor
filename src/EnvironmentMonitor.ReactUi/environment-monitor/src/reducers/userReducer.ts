import { createSlice, PayloadAction } from "@reduxjs/toolkit";
/*
import { User } from "../models/user";
import { UserActionTypes, UserActions } from "../actions/userActions";
import { produce } from "immer";
import { AppState } from "../setup/appRootReducer";

export interface UserState {
  user: User | undefined;
  isLoggingIn: boolean;
  loggedIn: boolean;
}

const defaultState: UserState = {
  user: undefined,
  isLoggingIn: false,
  loggedIn: false,
};

export function userReducer(
  state: UserState = defaultState,
  action: UserActions
): UserState {
  switch (action.type) {
    case UserActionTypes.StoreUserInfo:
      state = produce(state, (draft) => {
        draft.user = action.userInfo;
        if (action.userInfo) {
          draft.loggedIn = true;
        } else {
          draft.loggedIn = false;
        }
      });
      break;

    case UserActionTypes.SetIsLoggingIn:
      state = produce(state, (draft) => {
        draft.isLoggingIn = action.isLoggingIn;
      });
      break;
  }
  return state;
}

export const getUserInfo = (state: AppState): User | undefined =>
  state.userInfo.user;

export const getIsLoggedIn = (state: AppState): boolean =>
  state.userInfo.user != undefined;

export const getIsLoggingIn = (state: AppState): boolean =>
  state.userInfo.isLoggingIn;
*/

import { User } from "../models/user";
import { RootState } from "../setup/appStore";

export interface UserState {
  user: User | undefined;
  isLoggingIn: boolean;
  loggedIn: boolean;
}

const initialState: UserState = {
  user: undefined,
  isLoggingIn: false,
  loggedIn: false,
};

export const userSlice = createSlice({
  name: "measurement",
  initialState,
  reducers: {
    storeUserInfo: (state, action: PayloadAction<User | undefined>) => {
      state.user = action.payload;
      if (!action.payload) {
        state.loggedIn = false;
      }
    },
    setIsLoggingIn: (state, action: PayloadAction<boolean>) => {
      state.isLoggingIn = action.payload;
    },
  },
});

export const { storeUserInfo, setIsLoggingIn } = userSlice.actions;

export const getUserInfo = (state: RootState): User | undefined =>
  state.userInfo.user;

export const getIsLoggedIn = (state: RootState): boolean =>
  state.userInfo.user !== undefined;

export const getIsLoggingIn = (state: RootState): boolean =>
  state.userInfo.isLoggingIn;

export default userSlice.reducer;
