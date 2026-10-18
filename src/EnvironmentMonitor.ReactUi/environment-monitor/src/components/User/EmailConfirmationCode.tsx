import { useState } from "react";
import { Alert, Button, Stack } from "@mui/material";
import { useApiHook } from "../../hooks/apiHook";
import { useNotification } from "../../hooks/useNotification";
import { EmailCodeDialog } from "./EmailCodeDialog";

export const EmailConfirmationCode = ({ onConfirmed }: { onConfirmed: () => Promise<void> }) => {
  const api = useApiHook().userHook;
  const notification = useNotification();
  const [open, setOpen] = useState(false);

  return (
    <Stack spacing={2}>
      <Alert severity="info">Confirm your email address to enable two-factor authentication.</Alert>
      <Button variant="outlined" onClick={() => setOpen(true)} sx={{ alignSelf: "flex-start" }}>Confirm email</Button>
      <EmailCodeDialog
        open={open}
        title="Confirm email"
        onClose={() => setOpen(false)}
        onSend={async () => { await api.sendEmailConfirmationCode(); }}
        onConfirm={async (code) => {
          await api.confirmEmailCode({ code });
          notification.success("Email confirmed");
          await onConfirmed();
        }}
      />
    </Stack>
  );
};
