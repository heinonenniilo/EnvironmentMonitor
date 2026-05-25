import { Box } from "@mui/material";
import type { LocationModel } from "../../models/location";
import { InfoRow } from "../InfoRow";

export interface LocationInfoProps {
  location: LocationModel;
}

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
