import React, { useState } from "react";
import { useDispatch } from "react-redux";
import RegisterPage from "../components/User/RegisterPage";
import { addNotification } from "../reducers/userInterfaceReducer";
import { routes } from "../utilities/routes";
import { useNavigate } from "react-router";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useApiHook } from "../hooks/apiHook";

export const RegisterView: React.FC = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const apiHook = useApiHook();
  const [isLoading, setIsLoading] = useState(false);

  const handleRegister = async (email: string, password: string) => {
    setIsLoading(true);
    const message = await apiHook.userHook.register(email, password);

    if (message) {
      dispatch(
        addNotification({
          title:
            "Registration successful. Check your email for verification link.",
          body: "",
          severity: "success",
        })
      );

      setTimeout(() => {
        navigate(routes.main);
      }, 2000);
    }
    setIsLoading(false);
  };

  const handleNavigateToLogin = () => {
    navigate(routes.login);
  };

  return (
    <AppContentWrapper isLoading={isLoading}>
      <RegisterPage
        isLoading={isLoading}
        onRegister={handleRegister}
        onNavigateToLogin={handleNavigateToLogin}
      />
    </AppContentWrapper>
  );
};
