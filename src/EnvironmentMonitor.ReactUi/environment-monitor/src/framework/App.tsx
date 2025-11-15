import { Backdrop, Box, CircularProgress, Container } from "@mui/material";
import React, { useEffect, useState } from "react";
// import { getIsReLogging, getUserInfo, getUserVm } from "reducers/userReducer";
import { AdapterMoment } from "@mui/x-date-pickers/AdapterMoment";
import { LocalizationProvider } from "@mui/x-date-pickers";
import { CookiesProvider } from "react-cookie";
import { MenuBar } from "./MenuBar";
import {
  getIsLoggedIn,
  getUserInfo,
  storeUserInfo,
} from "../reducers/userReducer";
import { useApiHook } from "../hooks/apiHook";
import { useNavigate } from "react-router";
import { useDispatch, useSelector } from "react-redux";
import {
  getDevices,
  setDevices,
  setLocations,
  setSensors,
} from "../reducers/measurementReducer";
import {
  getConfirmationDialog,
  getNotifications,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";
import { ConfirmationDialog } from "./ConfirmationDialog";
import { NotificationsComponent } from "./NotificationsComponent";
import type { User } from "../models/user";
import { routes } from "../utilities/routes";

interface AppProps {
  children: React.ReactNode;
}

export const App: React.FC<AppProps> = (props) => {
  const navigate = useNavigate();
  // const [cookies, setCookie] = useCookies([userInfoCookieName]);
  const apiHook = useApiHook();
  const measurementApiHook = useApiHook().measureHook;
  const locationApiHook = useApiHook().locationHook;
  const dispath = useDispatch();
  const user: User | undefined = useSelector(getUserInfo);
  const isLoggedIn = useSelector(getIsLoggedIn);
  const [isLoading, setIsLoading] = useState(false);
  const [hasFetched, setHasFetched] = useState(false);
  const [inited, setInited] = useState(false);
  const notifications = useSelector(getNotifications);

  const devices = useSelector(getDevices);
  const confirmationDialogProps = useSelector(getConfirmationDialog);

  const handleLogOut = () => {
    apiHook.userHook.logOut().then(() => {
      dispath(setSensors([]));
      dispath(setDevices([]));
      dispath(storeUserInfo(undefined));
      navigate(routes.main);
    });
  };

  const handleNavigate = (route: string) => {
    navigate(route);
  };
  useEffect(() => {
    if (apiHook?.userHook && user === undefined && !hasFetched) {
      setIsLoading(true);
      apiHook.userHook
        .getUserInfo()
        .then((res) => {
          dispath(storeUserInfo(res));
        })
        .catch((ex) => {
          console.error("Failed to fetch user information", ex);
          dispath(storeUserInfo(undefined));
        })
        .finally(() => {
          setHasFetched(true);
          setIsLoading(false);
        });
    }
  }, [apiHook.userHook, user, dispath, hasFetched]);

  // Devices & sensors
  useEffect(() => {
    if (isLoggedIn && measurementApiHook && !inited) {
      setInited(true);
      measurementApiHook.getDevices().then((res) => {
        dispath(setDevices(res ?? []));
      });

      locationApiHook.getLocations().then((res) => {
        dispath(setLocations(res));
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isLoggedIn, dispath, inited]);

  useEffect(() => {
    if (!devices || devices.length === 0) {
      dispath(setSensors([]));
    } else {
      measurementApiHook
        .getSensors(devices.map((x) => x.identifier))
        .then((res) => {
          dispath(setSensors(res));
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [devices, dispath]);

  return (
    <CookiesProvider>
      <LocalizationProvider dateAdapter={AdapterMoment} adapterLocale="es">
        <>
          <Backdrop
            sx={{ color: "#fff", zIndex: (theme) => theme.zIndex.drawer + 1 }}
            open={isLoading}
          >
            <CircularProgress color="inherit" />
          </Backdrop>
          <ConfirmationDialog
            isOpen={confirmationDialogProps !== undefined}
            body={confirmationDialogProps?.body ?? ""}
            title={confirmationDialogProps?.title ?? ""}
            onConfirm={() => {
              if (confirmationDialogProps?.onConfirm) {
                confirmationDialogProps.onConfirm();
              }
              dispath(setConfirmDialog(undefined));
            }}
            onClose={() => {
              dispath(setConfirmDialog(undefined));
            }}
          />
          <NotificationsComponent messages={notifications} />

          <Container maxWidth={"xl"} sx={{ top: 0 }}>
            <MenuBar
              handleLogOut={handleLogOut}
              handleNavigateTo={handleNavigate}
              user={user}
            />
          </Container>

          <Box
            sx={{
              display: "flex",
              flexDirection: "column",
              width: "100%",
              height: "100%",
              marginTop: "60px",
            }}
          >
            {props.children}
          </Box>
        </>
      </LocalizationProvider>
    </CookiesProvider>
  );
};
