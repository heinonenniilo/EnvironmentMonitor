import { Box } from "@mui/material";
import { InfoRow } from "../InfoRow";
import type { DeviceInfo } from "../../models/deviceInfo";
import { getFormattedDate } from "../../utilities/datetimeUtils";
import { useSelector } from "react-redux";
import { getLocations } from "../../reducers/measurementReducer";
import { routes } from "../../utilities/routes";

export interface DeviceInfoProps {
  device: DeviceInfo;
}

export const DeviceInfoComponent: React.FC<DeviceInfoProps> = ({ device }) => {
  const locations = useSelector(getLocations);

  const matchingLocation = locations.find(
    (l) => l.identifier === device.device.locationIdentifier,
  );
  return (
    <Box display="flex" flexDirection="column" gap={1} p={1}>
      <InfoRow label="Guid" value={device.device.identifier} />
      <InfoRow label="Identifier" value={device.deviceIdentifier} />
      <InfoRow label="Name" value={device.device.name} />
      <InfoRow label="Created" value={getFormattedDate(device.created, true)} />
      <InfoRow
        label="Updated"
        value={device.updated ? getFormattedDate(device.updated, true) : ""}
      />
      {matchingLocation && (
        <InfoRow
          label="Location"
          value={matchingLocation.name}
          to={`${routes.locations}/${matchingLocation.identifier}`}
        />
      )}
    </Box>
  );
};
