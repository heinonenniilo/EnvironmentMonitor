import { useState } from "react";
import { useDispatch } from "react-redux";
import {
  Alert,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from "@mui/material";
import EmailOutlined from "@mui/icons-material/EmailOutlined";
import SmartphoneOutlined from "@mui/icons-material/SmartphoneOutlined";
import { useApiHook } from "../../hooks/apiHook";
import { MfaMethod, type MfaStatus } from "../../models/user";
import type { MfaSetupDto } from "../../models/mfa";
import { storeMfaStatus, storeUserInfo } from "../../reducers/userReducer";
import { mfaErrorMessage, mfaMethodLabel } from "../../utilities/mfaUtils";
import { EmailConfirmationCode } from "./EmailConfirmationCode";
import { EmailCodeDialog } from "./EmailCodeDialog";
import { AuthenticatorAppSetup } from "./AuthenticatorAppSetup";

interface MfaSettingsProps {
  status?: MfaStatus;
  emailConfirmed: boolean;
}

export const MfaSettings = ({ status, emailConfirmed }: MfaSettingsProps) => {
  const api = useApiHook().userHook;
  const dispatch = useDispatch();
  const [setup, setSetup] = useState<MfaSetupDto>();
  const [disabling, setDisabling] = useState<MfaMethod>();
  const [code, setCode] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [emailSetupOpen, setEmailSetupOpen] = useState(false);

  const refreshUser = async () => {
    const user = await api.getUserInfo();
    if (!user)
      throw new Error("Could not refresh your profile. Please try again.");
    dispatch(storeUserInfo(user));
    if (user.emailConfirmed !== true) {
      setEmailSetupOpen(false);
      setSetup(undefined);
      setCode("");
    }
  };

  const refreshAfterEnrollmentError = async () => {
    // Preserve the original enrollment error even if the refresh fails.
    try {
      await refreshUser();
    } catch {
      /* The existing error remains visible. */
    }
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
    } catch (ex) {
      setError(mfaErrorMessage(ex, "Something went wrong."));
      await refreshAfterEnrollmentError();
    } finally {
      setBusy(false);
    }
  };

  const save = async () => {
    if (disabling === undefined && (emailConfirmed !== true || !setup)) return;
    setBusy(true);
    setError("");
    try {
      const result =
        disabling !== undefined
          ? await api.disableMfa(disabling, code.trim() || undefined)
          : await api.confirmMfa(setup!.method, code.trim());
      dispatch(storeMfaStatus(result));
      setMessage(
        disabling !== undefined
          ? "MFA method disabled."
          : "MFA method enabled.",
      );
      setSetup(undefined);
      setDisabling(undefined);
      setCode("");
    } catch (ex) {
      setError(mfaErrorMessage(ex, "Something went wrong."));
      if (disabling === undefined) await refreshAfterEnrollmentError();
    } finally {
      setBusy(false);
    }
  };

  const close = () => {
    if (busy) return;
    setSetup(undefined);
    setDisabling(undefined);
    setCode("");
    setError("");
  };
  const dialogOpen =
    (emailConfirmed === true && setup !== undefined) || disabling !== undefined;

  const handleEmailEnrollmentError = async (error: unknown) => {
    setError(mfaErrorMessage(error, "Something went wrong."));
    await refreshAfterEnrollmentError();
    throw error;
  };

  return (
    <Stack spacing={2} sx={{ p: 2 }}>
      <Typography>
        Require a verification code after signing in. You can enable both
        methods.
      </Typography>
      <Chip
        label={status?.twoFactorEnabled ? "MFA enabled" : "MFA disabled"}
        color={status?.twoFactorEnabled ? "success" : "default"}
        variant="outlined"
        size="small"
        sx={{ alignSelf: "flex-start" }}
      />
      {message && <Alert severity="success">{message}</Alert>}
      {error && !dialogOpen && <Alert severity="error">{error}</Alert>}
      {emailConfirmed !== true && (
        <EmailConfirmationCode onConfirmed={refreshUser} />
      )}
      <TableContainer sx={{ border: 1, borderColor: "divider", borderRadius: 2 }}>
        <Table aria-label="MFA verification methods" sx={{ minWidth: 420 }}>
          <TableHead sx={{ bgcolor: "action.hover" }}>
            <TableRow>
              <TableCell>Verification method</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Action</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {[MfaMethod.Authenticator, MfaMethod.Email].map((method) => {
              const enabled = status?.enabledMethods.includes(method) ?? false;
              return (
                <TableRow key={method} sx={{ "&:last-child td": { borderBottom: 0 } }}>
                  <TableCell>
                    <Stack direction="row" spacing={1.5} alignItems="center">
                      {method === MfaMethod.Email ? <EmailOutlined color="action" /> : <SmartphoneOutlined color="action" />}
                      <Box>
                        <Typography variant="body2" fontWeight={600}>{mfaMethodLabel(method)}</Typography>
                        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                          {method === MfaMethod.Email ? "Receive a sign-in code by email." : "Generate sign-in codes in your authenticator app."}
                        </Typography>
                      </Box>
                    </Stack>
                  </TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={enabled ? "Enabled" : "Disabled"}
                      color={enabled ? "success" : "default"}
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Button
                      variant={enabled ? "outlined" : "contained"}
                      color={enabled ? "inherit" : "primary"}
                      size="small"
                      aria-label={`${enabled ? "Disable" : "Enable"} ${mfaMethodLabel(method)}`}
                      sx={{ minWidth: 80 }}
                      disabled={busy || (!enabled && emailConfirmed !== true)}
                      onClick={() => {
                        if (enabled) {
                          setDisabling(method);
                          setCode("");
                          setError("");
                          setMessage("");
                        } else if (method === MfaMethod.Email) {
                          setError("");
                          setMessage("");
                          setEmailSetupOpen(true);
                        } else {
                          void beginSetup(method);
                        }
                      }}
                    >
                      {enabled ? "Disable" : "Enable"}
                    </Button>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>
      <EmailCodeDialog
        open={emailSetupOpen && emailConfirmed === true}
        title="Enable Email MFA"
        confirmLabel="Confirm and enable"
        resendCooldown={60}
        onClose={() => setEmailSetupOpen(false)}
        onSend={async () => {
          if (emailConfirmed !== true)
            throw new Error("Confirm your email address first.");
          try {
            await api.setupMfa(MfaMethod.Email);
            setError("");
          } catch (error) {
            await handleEmailEnrollmentError(error);
          }
        }}
        onConfirm={async (emailCode) => {
          if (emailConfirmed !== true)
            throw new Error("Confirm your email address first.");
          try {
            const result = await api.confirmMfa(MfaMethod.Email, emailCode);
            dispatch(storeMfaStatus(result));
            setError("");
            setMessage("MFA method enabled.");
          } catch (error) {
            await handleEmailEnrollmentError(error);
          }
        }}
      />
      {emailConfirmed === true && setup && (
        <AuthenticatorAppSetup
          setup={setup}
          code={code}
          busy={busy}
          error={error}
          onCodeChange={setCode}
          onConfirm={() => void save()}
          onClose={close}
        />
      )}
      <Dialog
        open={disabling !== undefined}
        onClose={close}
        fullWidth
        maxWidth="sm"
      >
        <Box
          component="form"
          onSubmit={(event) => {
            event.preventDefault();
            void save();
          }}
        >
          <DialogTitle>
            Disable {mfaMethodLabel(disabling ?? MfaMethod.Email)}
          </DialogTitle>
          <DialogContent>
            <Stack spacing={2} sx={{ pt: 1 }}>
              {error && <Alert severity="error">{error}</Alert>}
              <Typography>
                Disable this verification method? You can optionally enter a
                current code for verification.
              </Typography>
              <TextField
                autoFocus
                label="Verification code (optional)"
                value={code}
                onChange={(event) => setCode(event.target.value)}
                disabled={busy}
                autoComplete="one-time-code"
                slotProps={{ htmlInput: { inputMode: "numeric" } }}
                fullWidth
              />
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button disabled={busy} onClick={close}>
              Cancel
            </Button>
            <Button
              type="submit"
              variant="contained"
              color="error"
              loading={busy}
            >
              Disable method
            </Button>
          </DialogActions>
        </Box>
      </Dialog>
    </Stack>
  );
};
