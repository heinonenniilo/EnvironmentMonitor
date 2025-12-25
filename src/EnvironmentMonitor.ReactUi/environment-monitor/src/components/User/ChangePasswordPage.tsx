import React, { useState } from "react";
import {
  Box,
  Button,
  Paper,
  TextField,
  Typography,
  Alert,
} from "@mui/material";
import { useNavigate } from "react-router";
import { useApiHook } from "../../hooks/apiHook";
import { routes } from "../../utilities/routes";

const ChangePasswordPage: React.FC = () => {
  const navigate = useNavigate();
  const apiHook = useApiHook();
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
      const message = await apiHook.userHook.changePassword(
        currentPassword,
        newPassword
      );
      setSuccess(message || "Password changed successfully!");
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");

      // Redirect to home after 3 seconds
      setTimeout(() => {
        navigate(routes.home);
      }, 3000);
    } catch (error) {
      // Error is already handled by apiHook
      console.error("Password change failed:", error);
    }
  };

  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        minHeight: "100vh",
        backgroundColor: "#f5f5f5",
      }}
    >
      <Paper
        elevation={3}
        sx={{
          padding: 4,
          maxWidth: 400,
          width: "100%",
        }}
      >
        <Typography variant="h4" component="h1" gutterBottom align="center">
          Change Password
        </Typography>

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
            disabled={!!success}
          />

          <TextField
            fullWidth
            type="password"
            label="New Password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            margin="normal"
            required
            disabled={!!success}
          />

          <TextField
            fullWidth
            type="password"
            label="Confirm New Password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            margin="normal"
            required
            disabled={!!success}
          />

          <Button
            fullWidth
            type="submit"
            variant="contained"
            color="primary"
            sx={{ mt: 2 }}
            disabled={!!success}
          >
            Change Password
          </Button>

          <Button
            fullWidth
            variant="text"
            color="primary"
            sx={{ mt: 1 }}
            onClick={() => navigate(routes.home)}
          >
            Cancel
          </Button>
        </form>
      </Paper>
    </Box>
  );
};

export default ChangePasswordPage;
