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
import { LocationMeasurementsView } from "./containers/LocationMeasurementsView";
import { DevicesView } from "./containers/DevicesView";
import { DeviceView } from "./containers/DeviceView";
import { store } from "./setup/appStore";
import { DeviceMessagesView } from "./containers/DeviceMessagesView";
import { ThemeProvider } from "@mui/material";
import { baseTheme } from "./utilities/baseTheme";
import { PublicMeasurementsView } from "./containers/PublicMeasurementsView";
import { LoginView } from "./containers/LoginView";

createRoot(document.getElementById("root")!).render(
  <Provider store={store}>
    <React.StrictMode>
      <BrowserRouter>
        <ThemeProvider theme={baseTheme}>
          <App>
            <Routes>
              <Route path={routes.main} element={<PublicMeasurementsView />} />
              <Route path={routes.home} element={<HomeView />} />
              <Route path={routes.login} element={<LoginView />} />
              <Route path={routes.dashboard} element={<DashboardView />} />
              <Route
                path={routes.locationDashboard}
                element={<DashbordLocationsView />}
              />
              <Route
                path={routes.measurements}
                element={<MeasurementsView />}
              />
              <Route
                path={routes.measurementsByDevice}
                element={<MeasurementsView />}
              />
              <Route
                path={routes.locationMeasurements}
                element={<LocationMeasurementsView />}
              />
              <Route
                path={routes.measurementsByLocation}
                element={<LocationMeasurementsView />}
              />
              <Route path={routes.devices} element={<DevicesView />} />
              <Route
                path={routes.deviceView}
                element={<DeviceView></DeviceView>}
              />
              <Route
                path={routes.deviceMessages}
                element={<DeviceMessagesView />}
              />
            </Routes>
          </App>
        </ThemeProvider>
      </BrowserRouter>
    </React.StrictMode>
  </Provider>
);
