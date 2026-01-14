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
import { LoginInfoView } from "./containers/LoginInfoView";
import { RegisterView } from "./containers/RegisterView";
import { EmailConfirmationView } from "./containers/EmailConfirmationView";
import { ForgotPasswordView } from "./containers/ForgotPasswordView";
import { ResetPasswordView } from "./containers/ResetPasswordView";
import { AuthorizedComponent } from "./components/AuthorizedComponent";
import { RoleNames } from "./enums/roleNames";
import { DeviceEmailsView } from "./containers/DeviceEmailsView";
import { UserInfoView } from "./containers/UserInfoView";
import { UsersView } from "./containers/UsersView";
import { UserView } from "./containers/UserView";

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
              <Route path={routes.loginInfo} element={<LoginInfoView />} />
              <Route path={routes.register} element={<RegisterView />} />
              <Route
                path={routes.emailConfirmation}
                element={<EmailConfirmationView />}
              />
              <Route
                path={routes.forgotPassword}
                element={<ForgotPasswordView />}
              />
              <Route
                path={routes.resetPassword}
                element={<ResetPasswordView />}
              />
              <Route
                path={routes.userInfo}
                element={
                  <AuthorizedComponent
                    roleLogic="OR"
                    requiredRoles={[
                      RoleNames.Registered,
                      RoleNames.User,
                      RoleNames.Admin,
                    ]}
                  >
                    <UserInfoView />
                  </AuthorizedComponent>
                }
              />
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
              <Route
                path={routes.deviceEmails}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.Admin}>
                    <DeviceEmailsView />
                  </AuthorizedComponent>
                }
              />
              <Route
                path={routes.users}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.Admin}>
                    <UsersView />
                  </AuthorizedComponent>
                }
              />
              <Route
                path={routes.userView}
                element={
                  <AuthorizedComponent requiredRole={RoleNames.Admin}>
                    <UserView />
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
