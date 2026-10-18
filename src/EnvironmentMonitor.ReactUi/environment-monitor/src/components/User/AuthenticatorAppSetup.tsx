import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { QRCodeSVG } from "qrcode.react";
import type { MfaSetupDto } from "../../models/mfa";

interface AuthenticatorAppSetupProps {
  setup: MfaSetupDto;
  code: string;
  busy: boolean;
  error: string;
  onCodeChange: (code: string) => void;
  onConfirm: () => void;
  onClose: () => void;
}

export const AuthenticatorAppSetup = ({
  setup,
  code,
  busy,
  error,
  onCodeChange,
  onConfirm,
  onClose,
}: AuthenticatorAppSetupProps) => (
  <Dialog
    open
    onClose={onClose}
    fullWidth
    maxWidth="sm"
    aria-labelledby="authenticator-setup-title"
  >
    <Box
      component="form"
      onSubmit={(event) => {
        event.preventDefault();
        onConfirm();
      }}
    >
      <DialogTitle id="authenticator-setup-title">
        Enable Authenticator app
      </DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          {error && <Alert severity="error">{error}</Alert>}
          <Typography>
            Scan this QR code with your authenticator app, then enter a code to
            confirm setup.
          </Typography>
          {setup.authenticatorUri && (
            <Box sx={{ bgcolor: "white", p: 2, alignSelf: "center" }}>
              <QRCodeSVG
                value={setup.authenticatorUri}
                size={200}
                marginSize={4}
                title="Authenticator setup QR code"
              />
            </Box>
          )}
          {setup.sharedKey && (
            <>
              <Typography>Manual setup key:</Typography>
              <Typography
                component="code"
                sx={{ overflowWrap: "anywhere", userSelect: "all" }}
              >
                {setup.sharedKey}
              </Typography>
            </>
          )}
          <TextField
            autoFocus
            label="Verification code"
            value={code}
            onChange={(event) => onCodeChange(event.target.value)}
            required
            disabled={busy}
            autoComplete="one-time-code"
            slotProps={{ htmlInput: { inputMode: "numeric" } }}
            fullWidth
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button disabled={busy} onClick={onClose}>
          Cancel
        </Button>
        <Button
          type="submit"
          variant="contained"
          loading={busy}
          disabled={!code.trim()}
        >
          Confirm and enable
        </Button>
      </DialogActions>
    </Box>
  </Dialog>
);
