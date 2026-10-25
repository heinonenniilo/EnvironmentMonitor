import { useEffect, useRef, useState } from "react";
import { Alert, Box, Button, FormControl, InputLabel, MenuItem, Select, Stack, TextField, Typography } from "@mui/material";
import { useNavigate } from "react-router";
import { useDispatch } from "react-redux";
import { useApiHook } from "../hooks/apiHook";
import { useCooldown } from "../hooks/useCooldown";
import { MfaMethod } from "../models/user";
import type { MfaChallengeDto } from "../models/mfa";
import { storeUserInfo } from "../reducers/userReducer";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { routes } from "../utilities/routes";
import { clearLoginPersistence, mfaErrorMessage, mfaMethodLabel, readLoginPersistence } from "../utilities/mfaUtils";

export const MfaLoginView = () => {
  const api = useApiHook().userHook;
  const apiRef = useRef(api);
  apiRef.current = api;
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const [challenge, setChallenge] = useState<MfaChallengeDto>();
  const [method, setMethod] = useState<MfaMethod>(MfaMethod.Authenticator);
  const [code, setCode] = useState("");
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [emailSent, setEmailSent] = useState(false);
  const cooldown = useCooldown();

  useEffect(() => {
    let active = true;
    apiRef.current.getMfaChallenge().then((result) => {
      if (!active) return;
      if (!result.challengePending) {
        clearLoginPersistence();
        navigate(routes.login, { replace: true });
        return;
      }
      const methods = result.availableMethods.filter((value) => value === MfaMethod.Email || value === MfaMethod.Authenticator);
      if (!methods.length) {
        setError("No supported MFA methods are available. Please sign in again.");
        return;
      }
      setChallenge({ ...result, availableMethods: methods });
      setMethod(methods.includes(MfaMethod.Authenticator) ? MfaMethod.Authenticator : methods[0]);
    }).catch((ex: unknown) => {
      if (active) setError(mfaErrorMessage(ex, "Could not load the login challenge. Please sign in again."));
    }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [navigate]);

  const sendEmail = async () => {
    setBusy(true);
    setError("");
    setMessage("");
    try {
      const result = await api.sendMfaEmailCode();
      setMessage(result.message || "Verification code sent. It expires after 5 minutes.");
      setEmailSent(true);
      cooldown.start();
    } catch (ex) { setError(mfaErrorMessage(ex, "Could not send a verification code.")); }
    finally { setBusy(false); }
  };

  const submit = async () => {
    setBusy(true);
    setError("");
    try {
      const result = await api.completeMfaLogin(method, code.trim(), readLoginPersistence());
      if (!result.success || result.requiresTwoFactor) {
        setError(result.message || "Verification failed. Check the code and try again.");
        return;
      }
      const user = await api.getUserInfo();
      if (!user) {
        setError("Could not load your session. Please sign in again.");
        return;
      }
      dispatch(storeUserInfo(user));
      clearLoginPersistence();
      navigate(routes.main, { replace: true });
    } catch (ex) { setError(mfaErrorMessage(ex, "Verification failed. The code may be invalid or expired.")); }
    finally { setBusy(false); }
  };

  return (
    <AppContentWrapper isLoading={loading}>
      <Box component="form" onSubmit={(event) => { event.preventDefault(); void submit(); }} sx={{ width: "100%", maxWidth: 400, mx: "auto", my: 6, p: 3 }}>
        <Stack spacing={2}>
          <Typography variant="h5">Verify your sign-in</Typography>
          {error && <Alert severity="error">{error}</Alert>}
          {challenge && <>
            <Typography>Enter a verification code to finish signing in{challenge.loginProvider ? ` with ${challenge.loginProvider}` : ""}.</Typography>
            <FormControl fullWidth disabled={busy}>
              <InputLabel id="mfa-method-label">Verification method</InputLabel>
              <Select labelId="mfa-method-label" label="Verification method" value={method} onChange={(event) => { setMethod(event.target.value as MfaMethod); setCode(""); setError(""); setMessage(""); }}>
                {challenge.availableMethods.map((value) => <MenuItem key={value} value={value}>{mfaMethodLabel(value)}</MenuItem>)}
              </Select>
            </FormControl>
            {method === MfaMethod.Email ? <>
              <Typography>Send a code to {challenge.maskedEmail || "your email address"}. Codes expire after 5 minutes.</Typography>
              <Button disabled={busy || cooldown.seconds > 0} onClick={() => void sendEmail()}>{cooldown.seconds > 0 ? `Resend in ${cooldown.seconds}s` : emailSent ? "Resend code" : "Send email code"}</Button>
              {message && <Alert severity="success">{message}</Alert>}
            </> : <Typography>Use the current code from your authenticator app.</Typography>}
            <TextField autoFocus label="Verification code" value={code} onChange={(event) => setCode(event.target.value)} autoComplete="one-time-code" required disabled={busy} slotProps={{ htmlInput: { inputMode: "numeric" } }} />
            <Button type="submit" variant="contained" loading={busy} disabled={!code.trim()}>Verify and sign in</Button>
          </>}
          <Button disabled={busy} onClick={() => { clearLoginPersistence(); navigate(routes.login, { replace: true }); }}>Back to login</Button>
        </Stack>
      </Box>
    </AppContentWrapper>
  );
};
