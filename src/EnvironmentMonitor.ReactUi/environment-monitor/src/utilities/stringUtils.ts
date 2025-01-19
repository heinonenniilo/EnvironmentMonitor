export const stringSort = (
  a: string | null | undefined,
  b: string | null | undefined
): number => {
  if (!a && !b) {
    return 0;
  }
  if (!a) {
    return 1;
  }
  if (!b) {
    return -1;
  }
  return a.localeCompare(b);
};
