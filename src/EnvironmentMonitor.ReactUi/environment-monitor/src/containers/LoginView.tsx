import LoginPage from "../components/LoginPage";

export const LoginView: React.FC = () => {
  const loginWithGoogleAuthCode = (persistent: boolean) => {
    if (process.env.NODE_ENV === "production") {
      window.location.href = `api/authentication/google?persistent=${persistent}`;
    } else {
      window.location.href = `https://localhost:7135/api/authentication/google?persistent=${persistent}`;
    }
  };

  return (
    <LoginPage
      onLoggedIn={() => window.location.reload()}
      onLogInWithGoogle={loginWithGoogleAuthCode}
    />
  );
};
