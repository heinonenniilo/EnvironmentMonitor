import { useState } from "react";
import { useNavigate } from "react-router";
import ChangePasswordPage from "../components/User/ChangePasswordPage";
import { useApiHook } from "../hooks/apiHook";
import { routes } from "../utilities/routes";
import { AppContentWrapper } from "../framework/AppContentWrapper";

export const ChangePasswordView = () => {
  const navigate = useNavigate();
  const apiHook = useApiHook();
  const [isLoading, setIsLoading] = useState(false);

  const handleChangePassword = async (
    currentPassword: string,
    newPassword: string
  ) => {
    setIsLoading(true);
    await apiHook.userHook.changePassword(currentPassword, newPassword);
    setIsLoading(false);

    // Redirect to home after 3 seconds
    setTimeout(() => {
      navigate(routes.home);
    }, 3000);
  };

  const handleCancel = () => {
    navigate(routes.home);
  };

  return (
    <AppContentWrapper isLoading={isLoading}>
      <ChangePasswordPage
        isLoading={isLoading}
        onChangePassword={handleChangePassword}
        onCancel={handleCancel}
      />
    </AppContentWrapper>
  );
};
