import { combineReducers } from "redux";
import { UserState, userReducer } from "../reducers/userReducer";
import {
  MeasurementState,
  measurementReducer,
} from "../reducers/measurementReducer";

export interface AppState {
  userInfo: UserState;
  measurementInfo: MeasurementState;
}

export const appRootReducer = combineReducers({
  userInfo: userReducer,
  measurementInfo: measurementReducer,
});
