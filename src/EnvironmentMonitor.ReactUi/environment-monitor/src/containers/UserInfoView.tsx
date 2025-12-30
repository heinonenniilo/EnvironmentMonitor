import { useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import { useNavigate } from "react-router";
import { getUserInfo, storeUserInfo } from "../reducers/userReducer";
import { UserInfoComponent } from "../components/User/UserInfoComponent";
import { useApiHook } from "../hooks/apiHook";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { addNotification } from "../reducers/userInterfaceReducer";
import { setConfirmDialog } from "../reducers/userInterfaceReducer";
import { routes } from "../utilities/routes";
import type { User } from "../models/user";

export const UserInfoView = () => {
  const user = useSelector(getUserInfo);
  const apiHook = useApiHook();
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);

  const handleChangePassword = async (
    currentPassword: string,
    newPassword: string
  ) => {
    setIsLoading(true);
    try {
      await apiHook.userHook.changePassword(currentPassword, newPassword);
      dispatch(
        addNotification({
          title: "Success",
          body: "Password changed successfully!",
          severity: "success",
        })
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleRemoveUser = (user: User) => {
    dispatch(
      setConfirmDialog({
        title: `Delete Account ${user.email}`,
        body: "Are you sure you want to delete your account? This action cannot be undone.",
        onConfirm: () => {
          dispatch(setConfirmDialog(undefined));
          setIsLoading(true);
          apiHook.userHook
            .deleteOwnUser()
            .then(() => {
              dispatch(storeUserInfo(undefined));
              dispatch(
                addNotification({
                  title: "Account removed",
                  body: "Account deleted successfully!",
                  severity: "success",
                })
              );
              navigate(routes.main);
            })
            .catch((error) => {
              console.error(error);
              dispatch(
                addNotification({
                  title: "Error",
                  body: "Failed to delete account. Please try again.",
                  severity: "error",
                })
              );
            })
            .finally(() => {
              setIsLoading(false);
            });
        },
      })
    );
  };

  if (!user) {
    return null;
  }

  return (
    <AppContentWrapper isLoading={isLoading} title="User Information">
      <UserInfoComponent
        user={user}
        isLoading={isLoading}
        onChangePassword={handleChangePassword}
        onRemove={handleRemoveUser}
      />
    </AppContentWrapper>
  );
};
