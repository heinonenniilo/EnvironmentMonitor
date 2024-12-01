import React from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import reportWebVitals from "./reportWebVitals";
import { Provider } from "react-redux";
import { BrowserRouter, Route, Routes } from "react-router";
import { routes } from "./utilities/routes";
import { HomeView } from "./containers/HomeView";
import { App } from "./framework/App";
import { MeasurementsView } from "./containers/MeasurementsView";
import { store } from "./setup/appStore";
import { DashboardView } from "./containers/DashboardView";

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement
);

root.render(
  <Provider store={store}>
    <React.StrictMode>
      <BrowserRouter>
        <App>
          <Routes>
            <Route path={routes.main} element={<HomeView />} />
            <Route path={routes.dashboard} element={<DashboardView />} />
            <Route path={routes.measurements} element={<MeasurementsView />} />
          </Routes>
        </App>
      </BrowserRouter>
    </React.StrictMode>
  </Provider>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
