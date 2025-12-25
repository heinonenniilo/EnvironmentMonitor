import { useDispatch } from "react-redux";
import RegisterPage from "../components/User/RegisterPage";
import { addNotification } from "../reducers/userInterfaceReducer";
import { routes } from "../utilities/routes";
import { useNavigate } from "react-router";

export const RegisterView: React.FC = () => {
  const dispath = useDispatch();
  const navigate = useNavigate();
  return (
    <RegisterPage
      onRegistered={() => {
        dispath(
          addNotification({
            title:
              "Registration successful. Check youf email for verification link.",
            body: "",
            severity: "success",
          })
        );
        navigate(routes.main);
      }}
    />
  );
};
