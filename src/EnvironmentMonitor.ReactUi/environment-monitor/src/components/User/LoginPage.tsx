import React, { useState } from "react";
import {
  TextField,
  Button,
  Box,
  Typography,
  FormControlLabel,
  Checkbox,
  Link,
  SvgIcon,
} from "@mui/material";
import { useApiHook } from "../../hooks/apiHook";
import { useNavigate } from "react-router";
import { routes } from "../../utilities/routes";

// Google Icon Component
const GoogleIcon = () => (
  <SvgIcon sx={{ width: 20, height: 20, mr: 1 }}>
    <svg viewBox="0 0 24 24">
      <path
        fill="#4285F4"
        d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
      />
      <path
        fill="#34A853"
        d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
      />
      <path
        fill="#FBBC05"
        d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
      />
      <path
        fill="#EA4335"
        d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
      />
    </svg>
  </SvgIcon>
);

// Microsoft Icon Component
const MicrosoftIcon = () => (
  <SvgIcon sx={{ width: 20, height: 20, mr: 1 }}>
    <svg viewBox="0 0 23 23">
      <path fill="#f3f3f3" d="M0 0h23v23H0z" />
      <path fill="#f35325" d="M1 1h10v10H1z" />
      <path fill="#81bc06" d="M12 1h10v10H12z" />
      <path fill="#05a6f0" d="M1 12h10v10H1z" />
      <path fill="#ffba08" d="M12 12h10v10H12z" />
    </svg>
  </SvgIcon>
);

export interface LoginPageProps {
  onLoggedIn: () => void;
  onLogInWithGoogle: (persistent: boolean) => void;
  onLogInWithMicrosoft: (persistent: boolean) => void;
}

const LoginPage: React.FC<LoginPageProps> = ({
  onLoggedIn,
  onLogInWithGoogle,
  onLogInWithMicrosoft,
}) => {
  const [userId, setUserId] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const apiHook = useApiHook();
  const navigate = useNavigate();

  const handleSubmit = async (e: any) => {
    e.preventDefault();
    setError("");

    try {
      setIsLoading(true);
      const res = await apiHook.userHook.logIn(userId, password, rememberMe);
      setIsLoading(false);
      if (res) {
        onLoggedIn();
      } else {
        setError("Login failed.");
      }
    } catch (err: any) {
      setIsLoading(false);
      // Handle errors
      console.error("Error logging in:", err);
      setError(err?.response?.data?.message || "Login failed.");
    }
  };

  const logInEnabled = userId.length > 4 && password.length > 4;

  return (
    <Box
      sx={{
        width: 300,
        margin: "50px auto",
        padding: "20px",
        border: "1px solid #ccc",
        borderRadius: "8px",
      }}
    >
      <Typography variant="h5" textAlign="center" mb={2}>
        Login
      </Typography>
      <form onSubmit={handleSubmit}>
        <TextField
          label="User ID"
          variant="outlined"
          fullWidth
          margin="normal"
          value={userId}
          onChange={(e) => setUserId(e.target.value)}
        />
        <TextField
          label="Password"
          type="password"
          variant="outlined"
          fullWidth
          margin="normal"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <Box sx={{ textAlign: "right", marginTop: 1 }}>
          <Link
            component="button"
            variant="body2"
            type="button"
            onClick={() => navigate(routes.forgotPassword)}
            sx={{ cursor: "pointer" }}
          >
            Forgot Password?
          </Link>
        </Box>
        {error && (
          <Typography color="error" variant="body2" mt={1}>
            {error}
          </Typography>
        )}
        <Button
          type="submit"
          variant="contained"
          color="primary"
          fullWidth
          disabled={!logInEnabled}
          loading={isLoading}
          sx={{ marginTop: 2 }}
        >
          Login
        </Button>
        <Button
          type="button"
          variant="outlined"
          fullWidth
          sx={{
            marginTop: 2,
            backgroundColor: "#fff",
            color: "#3c4043",
            border: "1px solid #dadce0",
            textTransform: "none",
            fontSize: "15px",
            fontWeight: 500,
            padding: "8px 12px",
            "&:hover": {
              backgroundColor: "#f7f8f8",
              border: "1px solid #dadce0",
            },
          }}
          onClick={() => {
            onLogInWithGoogle(rememberMe);
          }}
        >
          <GoogleIcon />
          Sign in with Google
        </Button>
        <Button
          type="button"
          variant="outlined"
          fullWidth
          sx={{
            marginTop: 2,
            backgroundColor: "#fff",
            color: "#5e5e5e",
            border: "1px solid #8c8c8c",
            textTransform: "none",
            fontSize: "15px",
            fontWeight: 500,
            padding: "8px 12px",
            "&:hover": {
              backgroundColor: "#f3f3f3",
              border: "1px solid #8c8c8c",
            },
          }}
          onClick={() => {
            onLogInWithMicrosoft(rememberMe);
          }}
        >
          <MicrosoftIcon />
          Sign in with Microsoft
        </Button>
        <FormControlLabel
          control={
            <Checkbox
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
              color="primary"
            />
          }
          label="Remember Me"
          sx={{ marginTop: 1 }}
        />
        <Button
          type="button"
          variant="outlined"
          color="secondary"
          fullWidth
          sx={{ marginTop: 2 }}
          onClick={() => navigate(routes.register)}
        >
          Register
        </Button>
      </form>
    </Box>
  );
};

export default LoginPage;
