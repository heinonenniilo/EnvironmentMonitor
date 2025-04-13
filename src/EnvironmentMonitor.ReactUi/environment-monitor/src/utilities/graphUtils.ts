export const getColor = (index: number): string => {
  const hue = (index * 137.508) % 360;
  const sat = 70;
  const light = index % 2 === 0 ? 45 : 65;
  return `hsl(${hue}, ${sat}%, ${light}%)`;
};
