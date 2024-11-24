import { Box, Container } from "@mui/material";
import React, { useEffect } from "react";
// import { getIsReLogging, getUserInfo, getUserVm } from "reducers/userReducer";
import { AdapterMoment } from "@mui/x-date-pickers/AdapterMoment";
import { LocalizationProvider } from "@mui/x-date-pickers";
import { CookiesProvider, useCookies } from "react-cookie";
import { routes } from "../utilities/routes";
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

interface AppProps {
  children: React.ReactNode;
}

// const userInfoCookieName = "userInfo";

export const App: React.FC<AppProps> = (props) => {
  const navigate = useNavigate();
  // const [cookies, setCookie] = useCookies([userInfoCookieName]);
  const apiHook = useApiHook();
  const dispath = useDispatch();
  const user: User | undefined = useSelector(getUserInfo);
  const isLoggedIn = useSelector(getIsLoggedIn);

  console.log(user);
  const handleLogOut = () => {
    console.info("Handling log out");
    apiHook.userHook.logOut().then(() => {
      dispath(storeUserInfo(undefined));
    });
  };

  const handleNavigate = (route: string) => {
    navigate(route);
  };
  useEffect(() => {
    if (apiHook?.userHook && user === undefined) {
      apiHook.userHook.getUserInfo().then((res) => {
        console.info(res);
        dispath(storeUserInfo(res));
      });
    }
  }, [apiHook.userHook, user, dispath]);

  return (
    <CookiesProvider>
      <LocalizationProvider dateAdapter={AdapterMoment}>
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
            marginTop: "80px",
          }}
        >
          {isLoggedIn ? (
            props.children
          ) : (
            <LoginPage
              onLogin={async () => {
                let res = await apiHook.userHook.getUserInfo();
                dispath(storeUserInfo(res));
              }}
            />
          )}
        </Box>
      </LocalizationProvider>
    </CookiesProvider>
  );
};
