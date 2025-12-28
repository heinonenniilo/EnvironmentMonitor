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
  ): Promise<boolean> => {
    setIsLoading(true);

    return apiHook.userHook
      .resetPassword(email, token, newPassword)
      .then(() => {
        return true;
      })
      .catch((er) => {
        console.error("Failed to reset password", er);
        return false;
      })
      .finally(() => {
        setIsLoading(false);
      });
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
