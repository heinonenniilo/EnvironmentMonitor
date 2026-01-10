import React, { useState } from "react";
import { TextField, Button, Box, Typography } from "@mui/material";

export interface RegisterPageProps {
  isLoading: boolean;
  hasRegistered?: boolean;
  onRegister: (email: string, password: string) => void;
  onNavigateToLogin: () => void;
}

const RegisterPage: React.FC<RegisterPageProps> = ({
  isLoading,
  hasRegistered,
  onRegister,
  onNavigateToLogin,
}) => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }
    onRegister(email, password);
  };

  const registerEnabled =
    email.length > 4 &&
    password.length > 4 &&
    confirmPassword.length > 4 &&
    !hasRegistered;

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
          onClick={onNavigateToLogin}
          disabled={isLoading}
        >
          Already have an account? Login
        </Button>
      </form>
    </Box>
  );
};

export default RegisterPage;
