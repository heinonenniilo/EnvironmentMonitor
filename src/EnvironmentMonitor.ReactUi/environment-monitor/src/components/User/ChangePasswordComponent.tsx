import React, { useState } from "react";
import { Button, Paper, TextField, Typography, Alert } from "@mui/material";

export interface ChangePasswordComponentProps {
  isLoading: boolean;
  onChangePassword: (
    currentPassword: string,
    newPassword: string
  ) => Promise<void>;
  title?: string;
}

export const ChangePasswordComponent: React.FC<
  ChangePasswordComponentProps
> = ({ isLoading, onChangePassword, title }) => {
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccess("");

    // Validation
    if (!currentPassword || !newPassword || !confirmPassword) {
      setError("All fields are required");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("New passwords do not match");
      return;
    }

    if (newPassword.length < 6) {
      setError("New password must be at least 6 characters long");
      return;
    }

    try {
      await onChangePassword(currentPassword, newPassword);
      setSuccess("Password changed successfully!");
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
    } catch (error: any) {
      console.error("Password change failed:", error);
      setError(error?.message || "Failed to change password.");
    }
  };

  return (
    <Paper
      elevation={3}
      sx={{
        padding: 4,
        maxWidth: 500,
        width: "100%",
      }}
    >
      {title && (
        <Typography variant="h5" component="h2" gutterBottom>
          {title}
        </Typography>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      <form onSubmit={handleSubmit}>
        <TextField
          fullWidth
          type="password"
          label="Current Password"
          value={currentPassword}
          onChange={(e) => setCurrentPassword(e.target.value)}
          margin="normal"
          required
          disabled={isLoading || !!success}
        />

        <TextField
          fullWidth
          type="password"
          label="New Password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          margin="normal"
          required
          disabled={isLoading || !!success}
        />

        <TextField
          fullWidth
          type="password"
          label="Confirm New Password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          margin="normal"
          required
          disabled={isLoading || !!success}
        />

        <Button
          fullWidth
          type="submit"
          variant="contained"
          color="primary"
          sx={{ mt: 2 }}
          disabled={isLoading || !!success}
        >
          {isLoading ? "Changing..." : "Change Password"}
        </Button>
      </form>
    </Paper>
  );
};
