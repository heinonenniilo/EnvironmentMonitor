export const deviceWarningLimitInMinutes = 10;
export const virtualDeviceWarningLimitInMinutes = 20;

export const getDeviceWarningLimitInMinutes = (isVirtual?: boolean): number => {
  return isVirtual
    ? virtualDeviceWarningLimitInMinutes
    : deviceWarningLimitInMinutes;
};

export const isTimestampWarning = (
  timestamp: Date | string | undefined | null,
  isVirtual?: boolean,
): boolean => {
  if (!timestamp) {
    return true;
  }

  const date = new Date(timestamp);
  if (Number.isNaN(date.getTime())) {
    return true;
  }

  return (
    Date.now() - date.getTime() >
    getDeviceWarningLimitInMinutes(isVirtual) * 60 * 1000
  );
};
