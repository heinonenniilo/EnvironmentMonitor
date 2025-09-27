UPDATE c
SET c.ClaimValue = d.Identifier
FROM application.AspNetUserClaims c
INNER JOIN Devices d ON d.Id = c.ClaimValue
WHERE c.ClaimType = 'Device';

UPDATE c
SET c.ClaimValue = l.Identifier
FROM application.AspNetUserClaims c
INNER JOIN Locations l ON l.Id = c.ClaimValue
WHERE c.ClaimType = 'Location';