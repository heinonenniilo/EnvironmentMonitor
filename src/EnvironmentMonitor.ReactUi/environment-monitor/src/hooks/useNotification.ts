import { useDispatch } from "react-redux";
import type { NotificationMessage } from "../framework/NotificationsComponent";
import { addNotification } from "../reducers/userInterfaceReducer";

type NotificationSeverity = NonNullable<NotificationMessage["severity"]>;

export const useNotification = () => {
  const dispatch = useDispatch();

  const add = (message: NotificationMessage) => {
    dispatch(addNotification(message));
  };

  const notify = (
    severity: NotificationSeverity,
    title: string,
    body = "",
    duration?: number,
  ) => {
    add({ title, body, severity, duration });
  };

  const success = (title: string, body = "", duration?: number) => {
    notify("success", title, body, duration);
  };

  const info = (title: string, body = "", duration?: number) => {
    notify("info", title, body, duration);
  };

  const warning = (title: string, body = "", duration?: number) => {
    notify("warning", title, body, duration);
  };

  const error = (title: string, body = "", duration?: number) => {
    notify("error", title, body, duration);
  };

  return {
    add,
    notify,
    success,
    info,
    warning,
    error,
  };
};
