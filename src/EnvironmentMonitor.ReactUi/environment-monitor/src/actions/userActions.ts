import { Action } from "redux";
import { User } from "../models/user";
export enum UserActionTypes {
  StoreUserInfo = "User/StoreUserInfo",
  SetIsLoggingIn = "User/IsLoggingIn",
}

export interface StoreUserInfo extends Action {
  type: UserActionTypes.StoreUserInfo;
  userInfo: User | undefined;
}

export interface SetIsUserLoggingIn extends Action {
  type: UserActionTypes.SetIsLoggingIn;
  isLoggingIn: boolean;
}

export const userActions = {
  storeUserInfo: (userInfo: User | undefined) => ({
    type: UserActionTypes.StoreUserInfo,
    userInfo,
  }),
  setIsLoggingIn: (isLoggingIn: boolean): SetIsUserLoggingIn => ({
    type: UserActionTypes.SetIsLoggingIn,
    isLoggingIn,
  }),
};

export type UserActions = StoreUserInfo | SetIsUserLoggingIn;
