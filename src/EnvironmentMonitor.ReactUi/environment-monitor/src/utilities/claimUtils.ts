import type { UserClaimDto } from "../models/userInfoDto";
import type { LocationModel } from "../models/location";
import type { Device } from "../models/device";
import type { ApiKeyClaimDto } from "../models/apiKey";

type ClaimDto = UserClaimDto | ApiKeyClaimDto | { type: string; value: string };

export const getClaimDisplayValue = (
  claim: ClaimDto,
  locations: LocationModel[],
  devices: Device[],
): string => {
  const claimTypeLower = claim.type.toLowerCase();

  if (claimTypeLower === "location" || claimTypeLower === "locationid") {
    const location = locations.find(
      (l) => l.identifier.toLowerCase() === claim.value.toLowerCase(),
    );
    return location ? location.name : claim.value;
  }

  if (claimTypeLower === "device" || claimTypeLower === "deviceid") {
    const device = devices.find(
      (d) => d.identifier.toLowerCase() === claim.value.toLowerCase(),
    );
    return device ? device.displayName || device.name : claim.value;
  }

  return claim.value;
};

export const sortClaims = (
  claims: ClaimDto[],
  locations: LocationModel[],
  devices: Device[],
): ClaimDto[] => {
  return [...claims].sort((a, b) => {
    const typeCompare = a.type.localeCompare(b.type);
    if (typeCompare !== 0) return typeCompare;
    return getClaimDisplayValue(a, locations, devices).localeCompare(
      getClaimDisplayValue(b, locations, devices),
    );
  });
};
