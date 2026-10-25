import { useId, useState } from "react";
import { Alert, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField, Typography } from "@mui/material";
import { useCooldown } from "../../hooks/useCooldown";
import { useNotification } from "../../hooks/useNotification";
import { mfaErrorMessage } from "../../utilities/mfaUtils";

interface EmailCodeDialogProps {
  open: boolean;
  title: string;
  confirmLabel?: string;
  resendCooldown?: number;
  onClose: () => void;
  onSend: () => Promise<void>;
  onConfirm: (code: string) => Promise<void>;
}

export const EmailCodeDialog = ({
  open, title, confirmLabel = "Confirm", resendCooldown = 30, onClose, onSend, onConfirm,
}: EmailCodeDialogProps) => {
  const titleId = useId();
  const notification = useNotification();
  const cooldown = useCooldown(resendCooldown);
  const [sent, setSent] = useState(false);
  const [code, setCode] = useState("");
  const [busy, setBusy] = useState(false);
  const [sendError, setSendError] = useState("");
  const [codeError, setCodeError] = useState("");
  const normalizedCode = code.replace(/[\s-]/g, "");
  const validCode = /^\d{6}$/.test(normalizedCode);

  const close = () => {
    if (busy) return;
    setCode("");
    setSendError("");
    setCodeError("");
    onClose();
  };

  const send = async () => {
    setBusy(true);
    setSendError("");
    try {
      await onSend();
      setSent(true);
      setCode("");
      setCodeError("");
      cooldown.start();
      notification.success("Code sent");
    } catch (error) {
      setSendError(mfaErrorMessage(error, "Something went wrong."));
    } finally { setBusy(false); }
  };

  const confirm = async () => {
    if (!validCode) return;
    setBusy(true);
    setCodeError("");
    try {
      await onConfirm(normalizedCode);
      setCode("");
      setSent(false);
      onClose();
    } catch (error) {
      setCodeError(mfaErrorMessage(error, "Something went wrong."));
    } finally { setBusy(false); }
  };

  return (
    <Dialog open={open} onClose={close} fullWidth maxWidth="sm" aria-labelledby={titleId}>
      <Box component="form" onSubmit={(event) => { event.preventDefault(); void confirm(); }}>
        <DialogTitle id={titleId}>{title}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {sendError && <Alert severity="error">{sendError}</Alert>}
            {!sent ? (
              <>
                <Typography>Send a verification code to your email address.</Typography>
                <Button onClick={() => void send()} loading={busy} disabled={cooldown.seconds > 0} sx={{ alignSelf: "flex-start" }}>
                  {cooldown.seconds > 0 ? `Send code in ${cooldown.seconds}s` : "Send code"}
                </Button>
              </>
            ) : (
              <>
                <Typography variant="body2" color="text.secondary">
                  The email may take a few seconds to arrive. Enter the 6-digit code; spaces and dashes are accepted.
                </Typography>
                <TextField
                  autoFocus
                  label="Verification code"
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
                <Button onClick={() => void send()} disabled={busy || cooldown.seconds > 0} sx={{ alignSelf: "flex-start" }}>
                  {cooldown.seconds > 0 ? `Resend code in ${cooldown.seconds}s` : "Resend code"}
                </Button>
              </>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button disabled={busy} onClick={close}>Close</Button>
          {sent && <Button type="submit" variant="contained" loading={busy} disabled={!validCode}>{confirmLabel}</Button>}
        </DialogActions>
      </Box>
    </Dialog>
  );
};
