import React, { useState } from "react";
import {
  TextField,
  Button,
  Box,
  Typography,
  Paper,
  Alert,
} from "@mui/material";

export interface ForgotPasswordPageProps {
  isLoading: boolean;
  onForgotPassword: (email: string) => Promise<void>;
  onNavigateToLogin: () => void;
}

const ForgotPasswordPage: React.FC<ForgotPasswordPageProps> = ({
  isLoading,
  onForgotPassword,
  onNavigateToLogin,
}) => {
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccessMessage("");

    // Validate email
    if (!email || email.length < 5) {
      setError("Please enter a valid email address.");
      return;
    }

    try {
      await onForgotPassword(email);
      setSuccessMessage("Password reset email sent successfully!");
      setEmail("");
    } catch (err: any) {
      console.error("Error sending reset email:", err);
      setError(err?.message || "Failed to send reset email. Please try again.");
    }
  };

  const isFormValid = email.length >= 5;

  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        minHeight: "60vh",
        padding: 3,
      }}
    >
      <Paper
        elevation={3}
        sx={{
          maxWidth: 450,
          width: "100%",
          padding: 4,
        }}
      >
        <Typography variant="h5" textAlign="center" mb={2}>
          Forgot Password
        </Typography>

        <Typography
          variant="body2"
          color="text.secondary"
          mb={3}
          textAlign="center"
        >
          Enter your email address and we'll send you a link to reset your
          password.
        </Typography>

        <form onSubmit={handleSubmit}>
          <TextField
            label="Email Address"
            type="email"
            variant="outlined"
            fullWidth
            margin="normal"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            disabled={isLoading || !!successMessage}
            required
            autoFocus
          />

          {error && (
            <Alert severity="error" sx={{ marginTop: 2 }}>
              {error}
            </Alert>
          )}

          {successMessage && (
            <Alert severity="success" sx={{ marginTop: 2 }}>
              {successMessage}
            </Alert>
          )}

          <Button
            type="submit"
            variant="contained"
            color="primary"
            fullWidth
            disabled={!isFormValid || isLoading || !!successMessage}
            sx={{ marginTop: 3 }}
          >
            {isLoading ? "Sending..." : "Send Reset Link"}
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
            Back to Login
          </Button>
        </form>
      </Paper>
    </Box>
  );
};

export default ForgotPasswordPage;
