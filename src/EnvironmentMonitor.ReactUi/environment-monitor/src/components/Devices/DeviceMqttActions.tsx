import { useState, type FormEvent } from "react";
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
} from "@mui/material";
import { ConfirmationDialog } from "../../framework/ConfirmationDialog";
import { useApiHook } from "../../hooks/apiHook";
import { useNotification } from "../../hooks/useNotification";

interface DeviceMqttActionsProps {
  identifier: string;
  deviceIdentifier: string;
}

const minimumPasswordLength = 6;

export const DeviceMqttActions: React.FC<DeviceMqttActionsProps> = ({
  identifier,
  deviceIdentifier,
}) => {
  const { registerMqttDevice, unregisterMqttDevice } = useApiHook().deviceHook;
  const notification = useNotification();
  const [dialog, setDialog] = useState<"register" | "unregister" | null>(null);
  const [password, setPassword] = useState("");
  const [passwordConfirm, setPasswordConfirm] = useState("");
  const [registrationError, setRegistrationError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const passwordsDoNotMatch =
    passwordConfirm.length > 0 && password !== passwordConfirm;
  const canRegister =
    password.length >= minimumPasswordLength && password === passwordConfirm;

  const clearPasswords = () => {
    setPassword("");
    setPasswordConfirm("");
  };

  const closeDialog = () => {
    clearPasswords();
    setRegistrationError("");
    setDialog(null);
  };

  const handleRegister = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!canRegister || isSubmitting) {
      return;
    }

    setIsSubmitting(true);
    setRegistrationError("");
    clearPasswords();

    try {
      await registerMqttDevice(identifier, { password, passwordConfirm });
      closeDialog();
      notification.success("Device registered to MQTT");
    } catch (error) {
      setRegistrationError(
        error instanceof Error ? error.message : "MQTT operation failed",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUnregister = async () => {
    if (isSubmitting) {
      return;
    }

    setIsSubmitting(true);
    try {
      await unregisterMqttDevice(identifier);
      notification.success("Device unregistered from MQTT");
    } catch (error) {
      notification.error(
        error instanceof Error ? error.message : "MQTT operation failed",
      );
    } finally {
      closeDialog();
      setIsSubmitting(false);
    }
  };

  return (
    <>
      <Stack direction="row" gap={1} flexWrap="wrap">
        <Button variant="outlined" onClick={() => setDialog("register")}>
          Register to MQTT
        </Button>
        <Button
          variant="outlined"
          color="error"
          onClick={() => setDialog("unregister")}
        >
          Unregister from MQTT
        </Button>
      </Stack>
      <Dialog
        open={dialog === "register"}
        onClose={isSubmitting ? undefined : closeDialog}
        maxWidth="xs"
        fullWidth
      >
        <Box
          component="form"
          onSubmit={handleRegister}
          display="flex"
          flexDirection="column"
          overflow="hidden"
        >
          <DialogTitle>Register to MQTT</DialogTitle>
          <DialogContent>
            <Stack spacing={2} pt={1}>
              {registrationError && (
                <Alert severity="error">{registrationError}</Alert>
              )}
              <TextField
                label="Device identifier"
                value={deviceIdentifier}
                slotProps={{ input: { readOnly: true } }}
              />
              <TextField
                autoFocus
                label="Password"
                type="password"
                autoComplete="new-password"
                required
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                helperText={`Use at least ${minimumPasswordLength} characters`}
                disabled={isSubmitting}
              />
              <TextField
                label="Confirm password"
                type="password"
                autoComplete="new-password"
                required
                value={passwordConfirm}
                onChange={(event) => setPasswordConfirm(event.target.value)}
                error={passwordsDoNotMatch}
                helperText={
                  passwordsDoNotMatch ? "Passwords do not match" : undefined
                }
                disabled={isSubmitting}
              />
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={closeDialog} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button
              type="submit"
              variant="contained"
              loading={isSubmitting}
              disabled={!canRegister}
            >
              Send
            </Button>
          </DialogActions>
        </Box>
      </Dialog>
      <ConfirmationDialog
        title="Unregister from MQTT"
        body="This deletes the device's MQTT credentials. The device can no longer connect until it is registered again."
        isOpen={dialog === "unregister"}
        isLoading={isSubmitting}
        onClose={closeDialog}
        onConfirm={handleUnregister}
      />
    </>
  );
};
