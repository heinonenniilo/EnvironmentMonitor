import React, { useState } from "react";
import {
  Box,
  Button,
  Paper,
  TextField,
  Typography,
  Alert,
} from "@mui/material";
import { useSearchParams } from "react-router";

export interface ResetPasswordPageProps {
  isLoading: boolean;
  onResetPassword: (
    email: string,
    token: string,
    newPassword: string
  ) => Promise<boolean>;
  onNavigateToLogin: () => void;
}

const ResetPasswordPage: React.FC<ResetPasswordPageProps> = ({
  isLoading,
  onResetPassword,
  onNavigateToLogin,
}) => {
  const [searchParams] = useSearchParams();

  const token = searchParams.get("token") || "";
  const isPasswordResetLink = !!token;

  const [email, setEmail] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  // If no email/token, show message that this requires a password reset link
  if (!isPasswordResetLink) {
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
            textAlign: "center",
          }}
        >
          <Typography variant="h5" mb={3}>
            Password Reset Required
          </Typography>
          <Alert severity="warning" sx={{ marginBottom: 3 }}>
            This page requires a password reset link. If you're trying to change
            your password, please request a password reset email first.
          </Alert>
          <Typography variant="body1" color="text.secondary" mb={3}>
            To reset your password, please use the "Forgot Password" option on
            the login page. You'll receive an email with a link to reset your
            password.
          </Typography>
          <Button
            variant="contained"
            color="primary"
            fullWidth
            onClick={onNavigateToLogin}
            sx={{ marginBottom: 1 }}
          >
            Go to Login
          </Button>
        </Paper>
      </Box>
    );
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccessMessage("");

    // Validate email is present
    if (!email || !email.trim()) {
      setError("Please enter your email address.");
      return;
    }

    // Validate email format
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      setError("Please enter a valid email address.");
      return;
    }

    // Validate passwords match
    if (newPassword !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    // Validate password length
    if (newPassword.length < 6) {
      setError("Password must be at least 6 characters long.");
      return;
    }

    // Validate token is present
    if (!token) {
      setError(
        "Invalid reset password link. Please request a new password reset."
      );
      return;
    }

    if (await onResetPassword(email, token, newPassword)) {
      setSuccessMessage(
        "Password reset successfully! Navigate to login and login with the new password."
      );
    } else {
      setError("Failed to reset password.");
    }
  };

  const isFormValid =
    email.trim().length > 0 &&
    newPassword.length >= 6 &&
    confirmPassword.length >= 6;

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
        <Typography variant="h5" textAlign="center" mb={3}>
          Reset Password
        </Typography>

        <Typography variant="body2" color="text.secondary" mb={3}>
          You are here because you requested to reset your password. Enter your
          email address and the new password.
        </Typography>

        <form onSubmit={handleSubmit}>
          <TextField
            label="Email"
            type="email"
            variant="outlined"
            fullWidth
            margin="normal"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            disabled={isLoading || !!successMessage}
            required
          />
          <TextField
            label="New Password"
            type="password"
            variant="outlined"
            fullWidth
            margin="normal"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            disabled={isLoading || !!successMessage}
            required
          />
          <TextField
            label="Confirm New Password"
            type="password"
            variant="outlined"
            fullWidth
            margin="normal"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            disabled={isLoading || !!successMessage}
            required
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
            {isLoading ? "Resetting..." : "Reset Password"}
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

export default ResetPasswordPage;
