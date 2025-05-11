import { createRoot } from "react-dom/client";
import "./index.css";
import { Provider } from "react-redux";
import React from "react";
import { BrowserRouter, Route, Routes } from "react-router";
import { App } from "./framework/App";
import { routes } from "./utilities/routes";
import { HomeView } from "./containers/HomeView";
import { DashboardView } from "./containers/DashboardView";
import { DashbordLocationsView } from "./containers/DashboardLocationsView";
import { MeasurementsView } from "./containers/MeasurementsView";
import { DevicesView } from "./containers/DevicesView";
import { DeviceView } from "./containers/DeviceView";
import { store } from "./setup/appStore";

createRoot(document.getElementById("root")!).render(
  <Provider store={store}>
    <React.StrictMode>
      <BrowserRouter>
        <App>
          <Routes>
            <Route path={routes.main} element={<HomeView />} />
            <Route path={routes.dashboard} element={<DashboardView />} />
            <Route
              path={routes.locationDashboard}
              element={<DashbordLocationsView />}
            />
            <Route path={routes.measurements} element={<MeasurementsView />} />
            <Route
              path={routes.measurementsByDevice}
              element={<MeasurementsView />}
            />
            <Route path={routes.devices} element={<DevicesView />} />
            <Route
              path={routes.deviceView}
              element={<DeviceView></DeviceView>}
            />
          </Routes>
        </App>
      </BrowserRouter>
    </React.StrictMode>
  </Provider>
);
