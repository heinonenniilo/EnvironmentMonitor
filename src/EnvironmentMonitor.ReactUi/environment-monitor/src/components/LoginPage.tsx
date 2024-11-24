import React, { useState } from "react";
import { TextField, Button, Box, Typography } from "@mui/material";
import { useApiHook } from "../hooks/apiHook";

export interface LoginPageProps {
  onLogin: () => void;
}

const LoginPage: React.FC<LoginPageProps> = ({ onLogin }) => {
  const [userId, setUserId] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const apiHook = useApiHook();

  const handleSubmit = async (e: any) => {
    e.preventDefault();
    setError("");
    try {
      let res = await apiHook.userHook.logIn(userId, password);
      console.info(res);
      if (res) {
        onLogin();
      } else {
        setError("Login failed.");
      }
    } catch (err: any) {
      // Handle errors
      console.error("Error logging in:", err);
      setError(err?.response?.data?.message || "Login failed.");
    }
  };

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
        Login
      </Typography>
      <form onSubmit={handleSubmit}>
        <TextField
          label="User ID"
          variant="outlined"
          fullWidth
          margin="normal"
          value={userId}
          onChange={(e) => setUserId(e.target.value)}
        />
        <TextField
          label="Password"
          type="password"
          variant="outlined"
          fullWidth
          margin="normal"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
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
          sx={{ marginTop: 2 }}
        >
          Login
        </Button>
      </form>
    </Box>
  );
};

export default LoginPage;
