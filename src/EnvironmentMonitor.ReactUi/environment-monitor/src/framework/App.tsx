import { Backdrop, Box, CircularProgress, Container } from "@mui/material";
import React, { useEffect, useState } from "react";
// import { getIsReLogging, getUserInfo, getUserVm } from "reducers/userReducer";
import { AdapterMoment } from "@mui/x-date-pickers/AdapterMoment";
import { LocalizationProvider } from "@mui/x-date-pickers";
import { CookiesProvider } from "react-cookie";
import { MenuBar } from "./MenuBar";
import { User } from "../models/user";
import {
  getIsLoggedIn,
  getUserInfo,
  storeUserInfo,
} from "../reducers/userReducer";
import LoginPage from "../components/LoginPage";
import { useApiHook } from "../hooks/apiHook";
import { useNavigate } from "react-router";
import { useDispatch, useSelector } from "react-redux";
import {
  getDevices,
  setDevices,
  setSensors,
} from "../reducers/measurementReducer";
import {
  getConfirmationDialog,
  getNotifications,
  setConfirmDialog,
} from "../reducers/userInterfaceReducer";
import { ConfirmationDialog } from "./ConfirmationDialog";
import { NotificationsComponent } from "./NotificationsComponent";

interface AppProps {
  children: React.ReactNode;
}

export const App: React.FC<AppProps> = (props) => {
  const navigate = useNavigate();
  // const [cookies, setCookie] = useCookies([userInfoCookieName]);
  const apiHook = useApiHook();
  const measurementApiHook = useApiHook().measureHook;
  const dispath = useDispatch();
  const user: User | undefined = useSelector(getUserInfo);
  const isLoggedIn = useSelector(getIsLoggedIn);
  const [isLoading, setIsLoading] = useState(false);
  const [hasFetched, setHasFetched] = useState(false);
  const [devicesFetched, setDevicesFetched] = useState(false);
  const notifications = useSelector(getNotifications);

  const devices = useSelector(getDevices);
  const confirmationDialogProps = useSelector(getConfirmationDialog);

  const handleLogOut = () => {
    console.info("Handling log out");
    apiHook.userHook.logOut().then(() => {
      dispath(setSensors([]));
      dispath(setDevices([]));
      dispath(storeUserInfo(undefined));
    });
  };

  const loginWithGoogleAuthCode = () => {
    if (process.env.NODE_ENV === "production") {
      window.location.href = "api/authentication/google";
    } else {
      window.location.href = "https://localhost:7135/api/authentication/google";
    }
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
          console.info(res);
          dispath(storeUserInfo(res));
        })
        .catch((ex) => {
          console.error("Failed to fetch user information");
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
    if (isLoggedIn && measurementApiHook && !devicesFetched) {
      setDevicesFetched(true);
      measurementApiHook.getDevices().then((res) => {
        dispath(setDevices(res ?? []));
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isLoggedIn, dispath, devicesFetched]);

  useEffect(() => {
    if (devices.length === 0) {
      dispath(setSensors([]));
    } else {
      measurementApiHook
        .getSensors(devices.map((x) => x.deviceIdentifier))
        .then((res) => {
          dispath(setSensors(res));
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [devices, dispath]);

  return (
    <CookiesProvider>
      <LocalizationProvider dateAdapter={AdapterMoment}>
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
          {isLoggedIn ? (
            <Container maxWidth={"xl"} sx={{ top: 0 }}>
              <MenuBar
                handleLogOut={handleLogOut}
                handleNavigateTo={handleNavigate}
                user={user}
              />
            </Container>
          ) : null}

          <Box
            sx={{
              display: "flex",
              flexDirection: "column",
              width: "100%",
              height: "100%",
              marginTop: "60px",
            }}
          >
            {isLoggedIn ? (
              props.children
            ) : (
              <LoginPage
                onLoggedIn={async () => {
                  let res = await apiHook.userHook.getUserInfo();
                  dispath(storeUserInfo(res));
                }}
                onLogInWithGoogle={loginWithGoogleAuthCode}
              />
            )}
          </Box>
        </>
      </LocalizationProvider>
    </CookiesProvider>
  );
};
