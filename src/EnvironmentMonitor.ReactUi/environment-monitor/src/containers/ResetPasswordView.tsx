import { useState } from "react";
import { useNavigate } from "react-router";
import ResetPasswordPage from "../components/User/ResetPasswordPage";
import { useApiHook } from "../hooks/apiHook";
import { routes } from "../utilities/routes";
import { AppContentWrapper } from "../framework/AppContentWrapper";

export const ResetPasswordView = () => {
  const navigate = useNavigate();
  const apiHook = useApiHook();
  const [isLoading, setIsLoading] = useState(false);

  const handleResetPassword = async (
    email: string,
    token: string,
    newPassword: string
  ) => {
    setIsLoading(true);
    await apiHook.userHook.resetPassword(email, token, newPassword);
    setIsLoading(false);

    // Redirect to login after 3 seconds
    setTimeout(() => {
      navigate(routes.login);
    }, 3000);
  };

  const handleNavigateToLogin = () => {
    navigate(routes.login);
  };

  return (
    <AppContentWrapper isLoading={isLoading}>
      <ResetPasswordPage
        isLoading={isLoading}
        onResetPassword={handleResetPassword}
        onNavigateToLogin={handleNavigateToLogin}
      />
    </AppContentWrapper>
  );
};
