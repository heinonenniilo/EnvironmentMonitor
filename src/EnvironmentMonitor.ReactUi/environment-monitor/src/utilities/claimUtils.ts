import type { UserClaimDto } from "../models/userInfoDto";
import type { LocationModel } from "../models/location";
import type { Device } from "../models/device";

export const getClaimDisplayValue = (
  claim: UserClaimDto,
  locations: LocationModel[],
  devices: Device[]
): string => {
  const claimTypeLower = claim.type.toLowerCase();

  if (claimTypeLower === "location") {
    const location = locations.find(
      (l) => l.identifier.toLowerCase() === claim.value.toLowerCase()
    );
    return location ? location.name : claim.value;
  }

  if (claimTypeLower === "device") {
    const device = devices.find(
      (d) => d.identifier.toLowerCase() === claim.value.toLowerCase()
    );
    return device ? device.displayName || device.name : claim.value;
  }

  return claim.value;
};

export const sortClaims = (
  claims: UserClaimDto[],
  locations: LocationModel[],
  devices: Device[]
): UserClaimDto[] => {
  return [...claims].sort((a, b) => {
    const typeCompare = a.type.localeCompare(b.type);
    if (typeCompare !== 0) return typeCompare;
    return getClaimDisplayValue(a, locations, devices).localeCompare(
      getClaimDisplayValue(b, locations, devices)
    );
  });
};
