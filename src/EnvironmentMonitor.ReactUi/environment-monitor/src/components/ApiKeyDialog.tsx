import React, { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
  Typography,
  Alert,
  IconButton,
  InputAdornment,
} from "@mui/material";
import { ContentCopy, Visibility, VisibilityOff } from "@mui/icons-material";

interface ApiKeyDialogProps {
  open: boolean;
  onClose: () => void;
  onGenerate: (description: string) => void;
  generatedKey?: {
    apiKey: string;
    id: string;
    description?: string;
    created: string;
  } | null;
  isLoading?: boolean;
}

export const ApiKeyDialog: React.FC<ApiKeyDialogProps> = ({
  open,
  onClose,
  onGenerate,
  generatedKey,
  isLoading,
}) => {
  const [description, setDescription] = useState("");
  const [showApiKey, setShowApiKey] = useState(true);
  const [copied, setCopied] = useState(false);

  const handleGenerate = () => {
    if (description.trim()) {
      onGenerate(description.trim());
    }
  };

  const handleClose = () => {
    setDescription("");
    setCopied(false);
    setShowApiKey(true);
    onClose();
  };

  const handleCopy = (text: string) => {
    navigator.clipboard.writeText(text);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>
        {generatedKey ? "API Key Generated" : "Generate Device API Key"}
      </DialogTitle>
      <DialogContent>
        {!generatedKey ? (
          <Box sx={{ mt: 2 }}>
            <TextField
              autoFocus
              fullWidth
              label="Description *"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Enter a description for this API key"
              helperText="Required: Provide a meaningful description to identify this API key"
              disabled={isLoading}
            />
          </Box>
        ) : (
          <Box sx={{ mt: 2 }}>
            <Alert severity="warning" sx={{ mb: 3 }}>
              <Typography variant="body2" fontWeight="bold">
                Important: Save this API key now!
              </Typography>
              <Typography variant="body2">
                This is the only time you will be able to see the secret key.
                Store it securely.
              </Typography>
            </Alert>

            <Box sx={{ mb: 2 }}>
              <Typography
                variant="subtitle2"
                color="text.secondary"
                gutterBottom
              >
                API Key ID
              </Typography>
              <TextField
                fullWidth
                value={generatedKey.id}
                InputProps={{
                  readOnly: true,
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => handleCopy(generatedKey.id)}
                        edge="end"
                        size="small"
                      >
                        <ContentCopy fontSize="small" />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
            </Box>

            <Box sx={{ mb: 2 }}>
              <Typography
                variant="subtitle2"
                color="text.secondary"
                gutterBottom
              >
                API Key Secret
              </Typography>
              <TextField
                fullWidth
                type={showApiKey ? "text" : "password"}
                value={generatedKey.apiKey}
                InputProps={{
                  readOnly: true,
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowApiKey(!showApiKey)}
                        edge="end"
                        size="small"
                      >
                        {showApiKey ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                      <IconButton
                        onClick={() => handleCopy(generatedKey.apiKey)}
                        edge="end"
                        size="small"
                      >
                        <ContentCopy fontSize="small" />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
            </Box>

            {generatedKey.description && (
              <Box sx={{ mb: 2 }}>
                <Typography
                  variant="subtitle2"
                  color="text.secondary"
                  gutterBottom
                >
                  Description
                </Typography>
                <Typography variant="body2">
                  {generatedKey.description}
                </Typography>
              </Box>
            )}

            {copied && (
              <Alert severity="success" sx={{ mt: 2 }}>
                Copied to clipboard!
              </Alert>
            )}
          </Box>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>
          {generatedKey ? "Close" : "Cancel"}
        </Button>
        {!generatedKey && (
          <Button
            onClick={handleGenerate}
            variant="contained"
            disabled={!description.trim() || isLoading}
          >
            Generate API Key
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
};
