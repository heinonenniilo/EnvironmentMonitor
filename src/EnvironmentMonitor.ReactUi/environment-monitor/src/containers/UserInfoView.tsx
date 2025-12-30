import { useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import { getUserInfo } from "../reducers/userReducer";
import { UserInfoComponent } from "../components/User/UserInfoComponent";
import { useApiHook } from "../hooks/apiHook";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { addNotification } from "../reducers/userInterfaceReducer";

export const UserInfoView = () => {
  const user = useSelector(getUserInfo);
  const apiHook = useApiHook();
  const dispatch = useDispatch();
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

  if (!user) {
    return null;
  }

  return (
    <AppContentWrapper isLoading={isLoading} title="User Information">
      <UserInfoComponent
        user={user}
        isLoading={isLoading}
        onChangePassword={handleChangePassword}
      />
    </AppContentWrapper>
  );
};
