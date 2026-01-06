import React, { useEffect, useState } from "react";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useApiHook } from "../hooks/apiHook";
import type { UserInfoDto } from "../models/userInfoDto";
import { UserTable } from "../components/UserManagement/UserTable";
import { useDispatch } from "react-redux";
import { addNotification } from "../reducers/userInterfaceReducer";

export const UsersView: React.FC = () => {
  const [users, setUsers] = useState<UserInfoDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const userManagementHook = useApiHook().userManagementHook;
  const dispatch = useDispatch();

  useEffect(() => {
    loadUsers();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadUsers = () => {
    setIsLoading(true);
    userManagementHook
      .getAllUsers()
      .then((res) => {
        setUsers(res);
      })
      .catch((error) => {
        console.error(error);
        dispatch(
          addNotification({
            title: "Failed to load users",
            body: "",
            severity: "error",
          })
        );
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  return (
    <AppContentWrapper title="Users" isLoading={isLoading}>
      <UserTable users={users} renderLink={true} />
    </AppContentWrapper>
  );
};
