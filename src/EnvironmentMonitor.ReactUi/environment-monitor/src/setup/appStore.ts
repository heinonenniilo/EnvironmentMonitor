import { appRootReducer } from "./appRootReducer";
import { createStore } from "redux";

export const appStore = createStore(
  appRootReducer,
  (window as any).__REDUX_DEVTOOLS_EXTENSION__ &&
    (window as any).__REDUX_DEVTOOLS_EXTENSION__()
);
