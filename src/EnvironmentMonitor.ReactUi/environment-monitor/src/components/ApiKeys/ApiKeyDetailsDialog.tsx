import React from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Divider,
  Chip,
} from "@mui/material";
import type { ApiKeyDto } from "../../models/apiKey";
import type { LocationModel } from "../../models/location";
import type { Device } from "../../models/device";
import moment from "moment";
import { getClaimDisplayValue } from "../../utilities/claimUtils";

interface ApiKeyDetailsDialogProps {
  open: boolean;
  apiKey: ApiKeyDto | null;
  onClose: () => void;
  locations: LocationModel[];
  devices: Device[];
}

export const ApiKeyDetailsDialog: React.FC<ApiKeyDetailsDialogProps> = ({
  open,
  apiKey,
  onClose,
  locations,
  devices,
}) => {
  if (!apiKey) {
    return null;
  }

  const deviceClaims = apiKey.claims.filter(
    (claim) => claim.type?.toLowerCase() === "device",
  );
  const locationClaims = apiKey.claims.filter(
    (claim) => claim.type?.toLowerCase() === "location",
  );

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>API Key Details</DialogTitle>
      <DialogContent>
        <Box sx={{ mt: 2 }}>
          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              ID
            </Typography>
            <Typography variant="body1" sx={{ fontFamily: "monospace" }}>
              {apiKey.id}
            </Typography>
          </Box>

          {apiKey.description && (
            <Box sx={{ mb: 3 }}>
              <Typography
                variant="subtitle2"
                color="text.secondary"
                gutterBottom
              >
                Description
              </Typography>
              <Typography variant="body1">{apiKey.description}</Typography>
            </Box>
          )}

          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              Created
            </Typography>
            <Typography variant="body1">
              {moment(apiKey.created).format("YYYY-MM-DD HH:mm:ss")}
            </Typography>
          </Box>

          {apiKey.updated && (
            <Box sx={{ mb: 3 }}>
              <Typography
                variant="subtitle2"
                color="text.secondary"
                gutterBottom
              >
                Updated
              </Typography>
              <Typography variant="body1">
                {moment(apiKey.updated).format("YYYY-MM-DD HH:mm:ss")}
              </Typography>
            </Box>
          )}

          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>
              Status
            </Typography>
            <Chip
              label={apiKey.enabled ? "Enabled" : "Disabled"}
              color={apiKey.enabled ? "success" : "default"}
              size="small"
            />
          </Box>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Claims
          </Typography>

          {deviceClaims.length > 0 && (
            <Box sx={{ mb: 3 }}>
              <Typography
                variant="subtitle2"
                color="text.secondary"
                gutterBottom
              >
                Device IDs ({deviceClaims.length})
              </Typography>
              <TableContainer
                component={Paper}
                variant="outlined"
                sx={{ mt: 1 }}
              >
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Device Name</TableCell>
                      <TableCell>Device ID</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {deviceClaims.map((claim, index) => (
                      <TableRow key={index}>
                        <TableCell>
                          {getClaimDisplayValue(claim, locations, devices)}
                        </TableCell>
                        <TableCell sx={{ fontFamily: "monospace" }}>
                          {claim.value}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
          )}

          {locationClaims.length > 0 && (
            <Box sx={{ mb: 3 }}>
              <Typography
                variant="subtitle2"
                color="text.secondary"
                gutterBottom
              >
                Location IDs ({locationClaims.length})
              </Typography>
              <TableContainer
                component={Paper}
                variant="outlined"
                sx={{ mt: 1 }}
              >
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Location Name</TableCell>
                      <TableCell>Location ID</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {locationClaims.map((claim, index) => (
                      <TableRow key={index}>
                        <TableCell>
                          {getClaimDisplayValue(claim, locations, devices)}
                        </TableCell>
                        <TableCell sx={{ fontFamily: "monospace" }}>
                          {claim.value}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
          )}

          {deviceClaims.length === 0 && locationClaims.length === 0 && (
            <Box sx={{ p: 2, textAlign: "center" }}>
              <Typography variant="body2" color="text.secondary">
                No claims found
              </Typography>
            </Box>
          )}

          {apiKey.claims.filter(
            (c) => c.type?.toLowerCase() !== "device" && c.type !== "location",
          ).length > 0 && (
            <Box sx={{ mb: 3 }}>
              <Typography
                variant="subtitle2"
                color="text.secondary"
                gutterBottom
              >
                Other Claims
              </Typography>
              <TableContainer
                component={Paper}
                variant="outlined"
                sx={{ mt: 1 }}
              >
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Type</TableCell>
                      <TableCell>Value</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {apiKey.claims
                      .filter(
                        (c) => c.type !== "DeviceId" && c.type !== "LocationId",
                      )
                      .map((claim, index) => (
                        <TableRow key={index}>
                          <TableCell>{claim.type}</TableCell>
                          <TableCell sx={{ fontFamily: "monospace" }}>
                            {claim.value}
                          </TableCell>
                        </TableRow>
                      ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
          )}
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
};
