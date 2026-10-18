import { useState } from "react";
import { useDispatch } from "react-redux";
import { Alert, Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField, Typography } from "@mui/material";
import { QRCodeSVG } from "qrcode.react";
import { useApiHook } from "../../hooks/apiHook";
import { useCooldown } from "../../hooks/useCooldown";
import { MfaMethod, type MfaStatus } from "../../models/user";
import type { MfaSetupDto } from "../../models/mfa";
import { storeMfaStatus, storeUserInfo } from "../../reducers/userReducer";
import { mfaErrorMessage, mfaMethodLabel } from "../../utilities/mfaUtils";
import { EmailConfirmationCode } from "./EmailConfirmationCode";

interface MfaSettingsProps {
  status?: MfaStatus;
  email: string;
  emailConfirmed: boolean;
}

export const MfaSettings = ({ status, email, emailConfirmed }: MfaSettingsProps) => {
  const api = useApiHook().userHook;
  const dispatch = useDispatch();
  const [setup, setSetup] = useState<MfaSetupDto>();
  const [disabling, setDisabling] = useState<MfaMethod>();
  const [code, setCode] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const cooldown = useCooldown();

  const refreshUser = async () => {
    const user = await api.getUserInfo();
    if (!user) throw new Error("Could not refresh your profile. Please try again.");
    dispatch(storeUserInfo(user));
    if (user.emailConfirmed !== true) {
      setSetup(undefined);
      setCode("");
    }
  };

  const refreshAfterEnrollmentError = async () => {
    // Preserve the original enrollment error even if the refresh fails.
    try { await refreshUser(); } catch { /* The existing error remains visible. */ }
  };

  const beginSetup = async (method: MfaMethod) => {
    if (emailConfirmed !== true) return;
    setBusy(true);
    setError("");
    setMessage("");
    setCode("");
    try {
      const result = await api.setupMfa(method);
      setSetup(result);
      if (result.codeSent) cooldown.start();
    } catch (ex) {
      setError(mfaErrorMessage(ex, "Something went wrong."));
      await refreshAfterEnrollmentError();
    } finally { setBusy(false); }
  };

  const save = async () => {
    if (disabling === undefined && (emailConfirmed !== true || !setup)) return;
    setBusy(true);
    setError("");
    try {
      const result = disabling !== undefined
        ? await api.disableMfa(disabling, code.trim() || undefined)
        : await api.confirmMfa(setup!.method, code.trim());
      dispatch(storeMfaStatus(result));
      setMessage(disabling !== undefined ? "MFA method disabled." : "MFA method enabled.");
      setSetup(undefined);
      setDisabling(undefined);
      setCode("");
    } catch (ex) {
      setError(mfaErrorMessage(ex, "Something went wrong."));
      if (disabling === undefined) await refreshAfterEnrollmentError();
    } finally { setBusy(false); }
  };

  const close = () => {
    if (busy) return;
    setSetup(undefined);
    setDisabling(undefined);
    setCode("");
    setError("");
  };
  const dialogOpen = (emailConfirmed === true && setup !== undefined) || disabling !== undefined;

  return (
    <Stack spacing={2} sx={{ p: 2 }}>
      <Typography>Require a verification code after signing in. You can enable both methods.</Typography>
      <Typography color="text.secondary">MFA {status?.twoFactorEnabled ? "enabled" : "disabled"}</Typography>
      {message && <Alert severity="success">{message}</Alert>}
      {error && !dialogOpen && <Alert severity="error">{error}</Alert>}
      {emailConfirmed !== true && <EmailConfirmationCode email={email} onConfirmed={refreshUser} />}
      {[MfaMethod.Authenticator, MfaMethod.Email].map((method) => {
        const enabled = status?.enabledMethods.includes(method) ?? false;
        return (
          <Stack key={method} direction="row" spacing={2} alignItems="center" flexWrap="wrap" useFlexGap>
            <Typography>{mfaMethodLabel(method)}</Typography>
            <Chip size="small" label={enabled ? "Enabled" : "Disabled"} color={enabled ? "success" : "default"} />
            <Button disabled={busy || (!enabled && emailConfirmed !== true)} onClick={() => {
              if (enabled) {
                setDisabling(method);
                setCode("");
                setError("");
                setMessage("");
              } else { void beginSetup(method); }
            }}>{enabled ? "Disable" : "Enable"}</Button>
          </Stack>
        );
      })}
      <Dialog open={dialogOpen} onClose={close} fullWidth maxWidth="sm">
        <Box component="form" onSubmit={(event) => { event.preventDefault(); void save(); }}>
          <DialogTitle>{disabling !== undefined ? "Disable" : "Enable"} {mfaMethodLabel(disabling ?? setup?.method ?? MfaMethod.Email)}</DialogTitle>
          <DialogContent>
            <Stack spacing={2} sx={{ pt: 1 }}>
              {error && <Alert severity="error">{error}</Alert>}
              {disabling !== undefined ? (
                <Typography>Disable this verification method? You can optionally enter a current code for verification.</Typography>
              ) : setup?.method === MfaMethod.Authenticator ? (
                <>
                  <Typography>Scan this QR code with your authenticator app, then enter a code to confirm setup.</Typography>
                  {setup.authenticatorUri && <Box sx={{ bgcolor: "white", p: 2, alignSelf: "center" }}><QRCodeSVG value={setup.authenticatorUri} size={200} marginSize={4} title="Authenticator setup QR code" /></Box>}
                  {setup.sharedKey && <><Typography>Manual setup key:</Typography><Typography component="code" sx={{ overflowWrap: "anywhere", userSelect: "all" }}>{setup.sharedKey}</Typography></>}
                </>
              ) : (
                <>
                  <Typography>{setup?.codeSent ? "A verification code was sent to your email." : "Request an email verification code."} Email codes expire after 5 minutes.</Typography>
                  <Button disabled={busy || cooldown.seconds > 0} onClick={() => void beginSetup(MfaMethod.Email)}>
                    {cooldown.seconds > 0 ? `Resend in ${cooldown.seconds}s` : "Resend code"}
                  </Button>
                </>
              )}
              <TextField autoFocus label={disabling !== undefined ? "Verification code (optional)" : "Verification code"} value={code} onChange={(event) => setCode(event.target.value)} required={disabling === undefined} disabled={busy} autoComplete="one-time-code" slotProps={{ htmlInput: { inputMode: "numeric" } }} fullWidth />
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button disabled={busy} onClick={close}>Cancel</Button>
            <Button type="submit" variant="contained" color={disabling !== undefined ? "error" : "primary"} loading={busy} disabled={disabling === undefined && !code.trim()}>{disabling !== undefined ? "Disable method" : "Confirm and enable"}</Button>
          </DialogActions>
        </Box>
      </Dialog>
    </Stack>
  );
};
