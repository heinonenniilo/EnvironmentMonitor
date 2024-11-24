import { AppContentWrapper } from "../framework/AppContentWrapper";
import React from "react";

export const HomeView: React.FC = () => {
  // const user = useSelector(getUserInfo);
  return (
    <AppContentWrapper titleParts={[{ text: "Home" }]}>
      <p>Please, login</p>
    </AppContentWrapper>
  );
};
