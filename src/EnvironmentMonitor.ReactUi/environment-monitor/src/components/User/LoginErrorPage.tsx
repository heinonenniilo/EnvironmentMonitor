import React from "react";
import { Box, Paper, Typography, Alert, Button } from "@mui/material";
import type { AuthInfoCookie } from "../../models/authInfoCookie";

export interface LoginErrorPageProps {
  authInfo: AuthInfoCookie | null;
  onNavigateToMain: () => void;
}

const LoginErrorPage: React.FC<LoginErrorPageProps> = ({
  authInfo,
  onNavigateToMain,
}) => {
  if (!authInfo) {
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
            padding: 4,
            maxWidth: 800,
            width: "100%",
            textAlign: "center",
          }}
        >
          <Typography variant="h5" gutterBottom>
            No Error Information Available
          </Typography>
          <Button
            variant="contained"
            color="primary"
            onClick={onNavigateToMain}
            sx={{ marginTop: 2 }}
          >
            Go to Main Page
          </Button>
        </Paper>
      </Box>
    );
  }

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
          padding: 4,
          maxWidth: 800,
          width: "100%",
        }}
      >
        <Typography variant="h5" gutterBottom>
          Login Failed
        </Typography>

        {authInfo.errorCode && (
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Error Code: {authInfo.errorCode}
          </Typography>
        )}

        <Box sx={{ marginTop: 2, marginBottom: 2 }}>
          {authInfo.errors.map((error, index) => (
            <Alert key={index} severity="error" sx={{ marginBottom: 1 }}>
              {error}
            </Alert>
          ))}
        </Box>

        <Button
          variant="contained"
          color="primary"
          fullWidth
          onClick={onNavigateToMain}
        >
          Go to Main Page
        </Button>
      </Paper>
    </Box>
  );
};

export default LoginErrorPage;
