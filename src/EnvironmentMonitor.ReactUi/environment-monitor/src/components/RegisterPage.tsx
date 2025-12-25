import React, { useState } from "react";
import { TextField, Button, Box, Typography } from "@mui/material";
import { useApiHook } from "../hooks/apiHook";
import { useNavigate } from "react-router";
import { routes } from "../utilities/routes";

export interface RegisterPageProps {
  onRegistered?: () => void;
}

const RegisterPage: React.FC<RegisterPageProps> = ({ onRegistered }) => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const apiHook = useApiHook();
  const navigate = useNavigate();

  const handleSubmit = async (e: any) => {
    e.preventDefault();
    setError("");
    setSuccessMessage("");

    // Validate password match
    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    try {
      setIsLoading(true);
      const message = await apiHook.userHook.register(email, password);
      setIsLoading(false);
      if (message) {
        setSuccessMessage(message);
        setEmail("");
        setPassword("");
        setConfirmPassword("");

        if (onRegistered) {
          onRegistered();
        }

        // Redirect to login page after 3 seconds
        setTimeout(() => {
          navigate(routes.login);
        }, 3000);
      }
    } catch (err: any) {
      setIsLoading(false);
      console.error("Error registering:", err);
      setError(err?.message || "Registration failed.");
    }
  };

  const registerEnabled =
    email.length > 4 && password.length > 4 && confirmPassword.length > 4;

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
        Register
      </Typography>
      <form onSubmit={handleSubmit}>
        <TextField
          label="Email"
          variant="outlined"
          fullWidth
          margin="normal"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          type="email"
          disabled={isLoading}
        />
        <TextField
          label="Password"
          type="password"
          variant="outlined"
          fullWidth
          margin="normal"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={isLoading}
        />
        <TextField
          label="Confirm Password"
          type="password"
          variant="outlined"
          fullWidth
          margin="normal"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          disabled={isLoading}
        />
        {error && (
          <Typography color="error" variant="body2" mt={1}>
            {error}
          </Typography>
        )}
        {successMessage && (
          <Typography color="success.main" variant="body2" mt={1}>
            {successMessage}
          </Typography>
        )}
        <Button
          type="submit"
          variant="contained"
          color="primary"
          fullWidth
          disabled={!registerEnabled || isLoading}
          sx={{ marginTop: 2 }}
        >
          {isLoading ? "Registering..." : "Register"}
        </Button>
        <Button
          type="button"
          variant="text"
          color="secondary"
          fullWidth
          sx={{ marginTop: 2 }}
          onClick={() => navigate(routes.login)}
          disabled={isLoading}
        >
          Already have an account? Login
        </Button>
      </form>
    </Box>
  );
};

export default RegisterPage;
