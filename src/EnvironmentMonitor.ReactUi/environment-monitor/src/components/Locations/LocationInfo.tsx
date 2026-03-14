import { Box, Checkbox, Typography } from "@mui/material";
import type { LocationModel } from "../../models/location";

export interface LocationInfoProps {
  location: LocationModel;
}

const labelWidth = 72;

interface InfoRowProps {
  label: string;
  value?: string | number;
  checked?: boolean;
}

const InfoRow: React.FC<InfoRowProps> = ({ label, value, checked }) => (
  <Box display="flex" alignItems="center" gap={2}>
    <Typography
      variant="body2"
      sx={{ minWidth: labelWidth, fontWeight: 600, color: "text.secondary" }}
    >
      {label}
    </Typography>
    {checked !== undefined ? (
      <Checkbox checked={checked} disabled size="small" sx={{ p: 0 }} />
    ) : (
      <Typography variant="body2">{value}</Typography>
    )}
  </Box>
);

export const LocationInfo: React.FC<LocationInfoProps> = ({ location }) => {
  return (
    <Box display="flex" flexDirection="column" gap={1} p={1}>
      <InfoRow label="Identifier" value={location.identifier} />
      <InfoRow label="Name" value={location.name} />
      <InfoRow label="Visible" checked={location.visible} />
      <InfoRow label="Sensors" value={location.locationSensors.length} />
      <InfoRow label="Devices" value={location.devices?.length ?? 0} />
    </Box>
  );
};
