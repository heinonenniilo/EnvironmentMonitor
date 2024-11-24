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
