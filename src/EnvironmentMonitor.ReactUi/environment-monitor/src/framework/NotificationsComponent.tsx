import { Alert, Snackbar, type SnackbarCloseReason } from "@mui/material";
import { useDispatch } from "react-redux";
import { removeNotification } from "../reducers/userInterfaceReducer";

export interface NotificationMessage {
  title: string;
  body: string;
  severity?: "success" | "info" | "warning" | "error";
  id?: number;
}

export interface NotificationMessageProps {
  messages: NotificationMessage[];
}

export const NotificationsComponent: React.FC<NotificationMessageProps> = ({
  messages,
}) => {
  const dispatch = useDispatch();
  const handleClose = (reason?: SnackbarCloseReason, id?: number) => {
    if (reason === "clickaway") {
      return;
    }
    dispatch(removeNotification(id));
  };

  return (
    <Snackbar
      open={messages.length > 0}
      autoHideDuration={6000}
      anchorOrigin={{ vertical: "top", horizontal: "center" }}
      onClose={(_x, y) => {
        handleClose(y);
      }}
    >
      <div>
        {messages.map((r) => {
          return (
            <Alert
              onClose={(_x) => {
                handleClose(undefined, r.id);
              }}
              severity={r.severity}
              variant="filled"
              sx={{ width: "100%", marginTop: 1 }}
              key={r.id}
            >
              {r.title}
            </Alert>
          );
        })}
      </div>
    </Snackbar>
  );
};
