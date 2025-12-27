import React, { useState } from "react";
import { TextField, Button, Box, Typography } from "@mui/material";

export interface RegisterPageProps {
  isLoading: boolean;
  onRegister: (email: string, password: string) => Promise<void>;
  onNavigateToLogin: () => void;
}

const RegisterPage: React.FC<RegisterPageProps> = ({
  isLoading,
  onRegister,
  onNavigateToLogin,
}) => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccessMessage("");

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    try {
      await onRegister(email, password);
      setSuccessMessage("Registration successful!");
      setEmail("");
      setPassword("");
      setConfirmPassword("");
    } catch (err: any) {
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
