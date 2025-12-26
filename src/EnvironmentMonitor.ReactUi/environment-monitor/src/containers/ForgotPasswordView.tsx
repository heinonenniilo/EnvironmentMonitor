import React, { useState } from "react";
import { useNavigate } from "react-router";
import ForgotPasswordPage from "../components/User/ForgotPasswordPage";
import { useApiHook } from "../hooks/apiHook";
import { routes } from "../utilities/routes";
import { AppContentWrapper } from "../framework/AppContentWrapper";

export const ForgotPasswordView: React.FC = () => {
  const navigate = useNavigate();
  const apiHook = useApiHook();
  const [isLoading, setIsLoading] = useState(false);

  const handleForgotPassword = async (email: string) => {
    setIsLoading(true);
    await apiHook.userHook.forgotPassword(email);
    setIsLoading(false);
  };

  const handleNavigateToLogin = () => {
    navigate(routes.login);
  };

  return (
    <AppContentWrapper isLoading={isLoading}>
      <ForgotPasswordPage
        isLoading={isLoading}
        onForgotPassword={handleForgotPassword}
        onNavigateToLogin={handleNavigateToLogin}
      />
    </AppContentWrapper>
  );
};
