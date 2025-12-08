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
import { AuthorizedComponent } from "./components/AuthorizedComponent";
import { RoleNames } from "./enums/roleNames";

createRoot(document.getElementById("root")!).render(
  <Provider store={store}>
    <React.StrictMode>
      <BrowserRouter>
        <ThemeProvider theme={baseTheme}>
          <App>
            <Routes>
              <Route path={routes.main} element={<PublicMeasurementsView />} />
              <Route
                path={routes.home}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.User}>
                    <HomeView />
                  </AuthorizedComponent>
                }
              />
              <Route path={routes.login} element={<LoginView />} />
              <Route
                path={routes.dashboard}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.User}>
                    <DashboardView />
                  </AuthorizedComponent>
                }
              />
              <Route
                path={routes.locationDashboard}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.User}>
                    <DashbordLocationsView />
                  </AuthorizedComponent>
                }
              />
              <Route
                path={routes.measurements}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.User}>
                    <MeasurementsView />
                  </AuthorizedComponent>
                }
              />
              <Route
                path={routes.measurementsByDevice}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.User}>
                    <MeasurementsView />
                  </AuthorizedComponent>
                }
              />
              <Route
                path={routes.locationMeasurements}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.User}>
                    <LocationMeasurementsView />
                  </AuthorizedComponent>
                }
              />
              <Route
                path={routes.measurementsByLocation}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.User}>
                    <LocationMeasurementsView />
                  </AuthorizedComponent>
                }
              />
              <Route
                path={routes.devices}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.Admin}>
                    <DevicesView />
                  </AuthorizedComponent>
                }
              />
              <Route
                path={routes.deviceView}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.Admin}>
                    <DeviceView />
                  </AuthorizedComponent>
                }
              />
              <Route
                path={routes.deviceMessages}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.Admin}>
                    <DeviceMessagesView />
                  </AuthorizedComponent>
                }
              />
            </Routes>
          </App>
        </ThemeProvider>
      </BrowserRouter>
    </React.StrictMode>
  </Provider>
);
