import React, { useState } from "react";
import {
  TextField,
  Button,
  Box,
  Typography,
  FormControlLabel,
  Checkbox,
} from "@mui/material";
import { useApiHook } from "../hooks/apiHook";
import { useNavigate } from "react-router";
import { routes } from "../utilities/routes";

export interface LoginPageProps {
  onLoggedIn: () => void;
  onLogInWithGoogle: (persistent: boolean) => void;
}

const LoginPage: React.FC<LoginPageProps> = ({
  onLoggedIn,
  onLogInWithGoogle,
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
          variant="contained"
          color="primary"
          fullWidth
          sx={{ marginTop: 2 }}
          onClick={() => {
            onLogInWithGoogle(rememberMe);
          }}
        >
          Google login
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
