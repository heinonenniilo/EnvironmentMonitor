import LoginPage from "../components/User/LoginPage";
import { useNavigate } from "react-router";
import { routes } from "../utilities/routes";
import { useApiHook } from "../hooks/apiHook";
import { useState } from "react";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useDispatch } from "react-redux";
import { storeUserInfo } from "../reducers/userReducer";
import { setDevices, setLocations } from "../reducers/measurementReducer";

export const LoginView: React.FC = () => {
  const navigate = useNavigate();
  const dispath = useDispatch();
  const apiHook = useApiHook();
  const measurementApiHook = useApiHook().measureHook;
  const locationApiHook = useApiHook().locationHook;
  const [isLoading, setIsLoading] = useState(false);

  const loginWithGoogleAuthCode = (persistent: boolean) => {
    if (process.env.NODE_ENV === "production") {
      window.location.href = `api/authentication/google?persistent=${persistent}`;
    } else {
      window.location.href = `https://localhost:7135/api/authentication/google?persistent=${persistent}`;
    }
  };

  const onLoggedIn = () => {
    setIsLoading(true);

    Promise.all([
      apiHook.userHook
        .getUserInfo()
        .then((res) => {
          dispath(storeUserInfo(res));
        })
        .catch((ex) => {
          console.error("Failed to fetch user information", ex);
          dispath(storeUserInfo(undefined));
        }),
      measurementApiHook
        .getDevices()
        .then((res) => {
          dispath(setDevices(res ?? []));
        })
        .catch((ex) => {
          console.error("Failed to fetch devices", ex);
        }),
      locationApiHook
        .getLocations()
        .then((res) => {
          dispath(setLocations(res));
        })
        .catch((ex) => {
          console.error("Failed to fetch locations", ex);
        }),
    ]).finally(() => {
      setIsLoading(false);
      navigate(routes.main);
    });
  };

  return (
    <AppContentWrapper title="Login" isLoading={isLoading}>
      <LoginPage
        onLoggedIn={onLoggedIn}
        onLogInWithGoogle={loginWithGoogleAuthCode}
      />
    </AppContentWrapper>
  );
};
