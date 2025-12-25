import React, { useState } from "react";
import {
  TextField,
  Button,
  Box,
  Typography,
  Paper,
  Alert,
} from "@mui/material";
import { useNavigate } from "react-router";
import { routes } from "../../utilities/routes";
import { useApiHook } from "../../hooks/apiHook";

const ForgotPasswordPage: React.FC = () => {
  const navigate = useNavigate();
  const apiHook = useApiHook();

  const [email, setEmail] = useState("");
  const [isLoading, setIsLoading] = useState(false);
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
      setIsLoading(true);
      const message = await apiHook.userHook.forgotPassword(email);
      setIsLoading(false);

      if (message) {
        setSuccessMessage(message);
        setEmail("");
      }
    } catch (err: any) {
      setIsLoading(false);
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
            onClick={() => navigate(routes.login)}
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
