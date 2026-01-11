import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router";
import LoginErrorPage from "../components/User/LoginErrorPage";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { routes } from "../utilities/routes";
import { useApiHook } from "../hooks/apiHook";
import type { AuthInfoCookie } from "../models/authInfoCookie";

export const LoginErrorView: React.FC = () => {
  const navigate = useNavigate();
  const apiHook = useApiHook();
  const [authInfo, setAuthInfo] = useState<AuthInfoCookie | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchAuthInfo = async () => {
      setIsLoading(true);
      const data = await apiHook.userHook.getAuthInfo();
      setAuthInfo(data);
      setIsLoading(false);
    };

    fetchAuthInfo();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleNavigateToMain = () => {
    navigate(routes.main);
  };

  return (
    <AppContentWrapper isLoading={isLoading}>
      <LoginErrorPage
        authInfo={authInfo}
        onNavigateToMain={handleNavigateToMain}
      />
    </AppContentWrapper>
  );
};
