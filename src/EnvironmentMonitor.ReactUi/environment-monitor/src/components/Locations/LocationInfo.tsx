import { Box, Typography } from "@mui/material";
import type { LocationModel } from "../../models/location";

export interface LocationInfoProps {
  location: LocationModel;
}

const labelWidth = 72;

const InfoRow: React.FC<{ label: string; value: string | number }> = ({
  label,
  value,
}) => (
  <Box display="flex" alignItems="baseline" gap={2}>
    <Typography
      variant="body2"
      sx={{ minWidth: labelWidth, fontWeight: 600, color: "text.secondary" }}
    >
      {label}
    </Typography>
    <Typography variant="body2">{value}</Typography>
  </Box>
);

export const LocationInfo: React.FC<LocationInfoProps> = ({ location }) => {
  return (
    <Box display="flex" flexDirection="column" gap={1} p={1}>
      <InfoRow label="Identifier" value={location.identifier} />
      <InfoRow label="Name" value={location.name} />
      <InfoRow label="Visible" value={location.visible ? "Yes" : "No"} />
      <InfoRow label="Sensors" value={location.locationSensors.length} />
      <InfoRow label="Devices" value={location.devices?.length ?? 0} />
    </Box>
  );
};
