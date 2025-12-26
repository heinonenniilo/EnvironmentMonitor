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
  ) => Promise<void>;
  onNavigateToLogin: () => void;
}

const ResetPasswordPage: React.FC<ResetPasswordPageProps> = ({
  isLoading,
  onResetPassword,
  onNavigateToLogin,
}) => {
  const [searchParams] = useSearchParams();

  const email = searchParams.get("email") || "";
  const token = searchParams.get("token") || "";
  const isPasswordResetLink = !!(email && token);

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

    // Validate email and token are present
    if (!email || !token) {
      setError(
        "Invalid reset password link. Please request a new password reset."
      );
      return;
    }

    try {
      await onResetPassword(email, token, newPassword);
      setSuccessMessage("Password reset successfully!");
      // Navigation will be handled by the view
    } catch (err: any) {
      console.error("Error resetting password:", err);
      setError(err?.message || "Failed to reset password. Please try again.");
    }
  };

  const isFormValid = newPassword.length >= 6 && confirmPassword.length >= 6;

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

        {email && (
          <Alert severity="info" sx={{ marginBottom: 2 }}>
            Resetting password for: <strong>{email}</strong>
          </Alert>
        )}

        <form onSubmit={handleSubmit}>
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
