import React from "react";
import { Box, Typography, Button, Paper } from "@mui/material";
import { useNavigate, useSearchParams } from "react-router";
import { routes } from "../utilities/routes";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";

const EmailConfirmationPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const status = searchParams.get("status");

  const isSuccess = status === "success";
  const isFailed = status === "failed";
  const isInvalid = status === "invalid";

  const getTitle = () => {
    if (isSuccess) return "Email Confirmed!";
    if (isFailed) return "Confirmation Failed";
    if (isInvalid) return "Invalid Link";
    return "Confirmation Failed";
  };

  const getMessage = () => {
    if (isSuccess) {
      return "Your email has been successfully confirmed. You can now log in to your account.";
    }
    if (isFailed) {
      return "We couldn't confirm your email. There was an error processing your confirmation. Please try registering again or contact support if the problem persists.";
    }
    if (isInvalid) {
      return "The confirmation link is invalid or has already been used. If you've already confirmed your email, you can proceed to login. Otherwise, please register again.";
    }
    return "We couldn't confirm your email. The link may have expired or is invalid. Please try registering again or contact support.";
  };

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
          maxWidth: 500,
          width: "100%",
          padding: 4,
          textAlign: "center",
        }}
      >
        {isSuccess ? (
          <CheckCircleOutlineIcon
            sx={{ fontSize: 80, color: "success.main", marginBottom: 2 }}
          />
        ) : (
          <ErrorOutlineIcon
            sx={{ fontSize: 80, color: "error.main", marginBottom: 2 }}
          />
        )}

        <Typography variant="h4" gutterBottom>
          {getTitle()}
        </Typography>

        <Typography
          variant="body1"
          color="text.secondary"
          sx={{ marginBottom: 3 }}
        >
          {getMessage()}
        </Typography>

        <Button
          variant="contained"
          color="primary"
          size="large"
          fullWidth
          onClick={() => navigate(routes.login)}
          sx={{ marginBottom: 1 }}
        >
          Go to Login
        </Button>

        {!isSuccess && (
          <Button
            variant="outlined"
            color="secondary"
            size="large"
            fullWidth
            onClick={() => navigate(routes.register)}
            sx={{ marginTop: 1 }}
          >
            Register Again
          </Button>
        )}
      </Paper>
    </Box>
  );
};

export default EmailConfirmationPage;
