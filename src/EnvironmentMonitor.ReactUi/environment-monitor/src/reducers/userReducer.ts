import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { User } from "../models/user";
import { RootState } from "../setup/appStore";
import { RoleNames } from "../enums/roleNames";

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

export const hasRole =
  (roleName: RoleNames) =>
  (state: RootState): boolean => {
    return state.userInfo.user?.roles.includes(roleName) ?? false;
  };

export default userSlice.reducer;
