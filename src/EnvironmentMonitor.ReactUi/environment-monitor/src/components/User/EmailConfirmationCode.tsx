import { useState } from "react";
import { Alert, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField, Typography } from "@mui/material";
import { useApiHook } from "../../hooks/apiHook";
import { useCooldown } from "../../hooks/useCooldown";
import { useNotification } from "../../hooks/useNotification";
import { mfaErrorMessage } from "../../utilities/mfaUtils";

interface EmailConfirmationCodeProps {
  email: string;
  onConfirmed: () => Promise<void>;
}

export const EmailConfirmationCode = ({ email, onConfirmed }: EmailConfirmationCodeProps) => {
  const api = useApiHook().userHook;
  const notification = useNotification();
  const cooldown = useCooldown(30);
  const [open, setOpen] = useState(false);
  const [sent, setSent] = useState(false);
  const [code, setCode] = useState("");
  const [busy, setBusy] = useState(false);
  const [sendError, setSendError] = useState("");
  const [codeError, setCodeError] = useState("");
  const normalizedCode = code.replace(/[\s-]/g, "");

  const send = async () => {
    setBusy(true);
    setSendError("");
    try {
      await api.sendEmailConfirmationCode();
      setSent(true);
      cooldown.start();
      notification.success(`Code sent to ${email}`);
    } catch (error) {
      setSendError(mfaErrorMessage(error, "Something went wrong."));
    } finally {
      setBusy(false);
    }
  };

  const confirm = async () => {
    if (!/^\d{6}$/.test(normalizedCode)) return;
    setBusy(true);
    setCodeError("");
    try {
      await api.confirmEmailCode({ code: normalizedCode });
      notification.success("Email confirmed");
      await onConfirmed();
      setOpen(false);
    } catch (error) {
      setCodeError(mfaErrorMessage(error, "Something went wrong."));
    } finally {
      setBusy(false);
    }
  };

  return (
    <Stack spacing={2}>
      <Alert severity="info">Confirm your email address to enable two-factor authentication.</Alert>
      <Button variant="outlined" onClick={() => setOpen(true)} sx={{ alignSelf: "flex-start" }}>Confirm email</Button>
      <Dialog open={open} onClose={() => { if (!busy) setOpen(false); }} fullWidth maxWidth="sm" aria-labelledby="confirm-email-title">
        <DialogTitle id="confirm-email-title">Confirm email</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <Typography>{email}</Typography>
            {sendError && <Alert severity="error">{sendError}</Alert>}
            {!sent ? (
              <Button onClick={() => void send()} loading={busy} sx={{ alignSelf: "flex-start" }}>Send code</Button>
            ) : (
              <Box component="form" onSubmit={(event) => { event.preventDefault(); void confirm(); }}>
                <Stack spacing={2}>
                  <Typography variant="body2" color="text.secondary">
                    The email may take a few seconds to arrive. Enter the 6-digit code; spaces and dashes are accepted.
                  </Typography>
                  <TextField
                    autoFocus
                    label="Email confirmation code"
                    value={code}
                    onChange={(event) => { setCode(event.target.value); setCodeError(""); }}
                    autoComplete="one-time-code"
                    slotProps={{ htmlInput: { inputMode: "numeric" } }}
                    error={Boolean(codeError)}
                    helperText={codeError}
                    disabled={busy}
                    required
                    fullWidth
                  />
                  <Stack direction="row" spacing={2}>
                    <Button type="submit" variant="contained" loading={busy} disabled={!/^\d{6}$/.test(normalizedCode)}>Confirm</Button>
                    <Button onClick={() => void send()} disabled={busy || cooldown.seconds > 0}>
                      {cooldown.seconds > 0 ? `Resend code in ${cooldown.seconds}s` : "Resend code"}
                    </Button>
                  </Stack>
                </Stack>
              </Box>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button disabled={busy} onClick={() => setOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
};
