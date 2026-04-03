import { Box, Paper, Typography } from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import {
  CircleMarker,
  MapContainer,
  TileLayer,
  Tooltip,
  useMap,
} from "react-leaflet";
import "leaflet/dist/leaflet.css";
import L from "leaflet";
import { MeasurementTypes } from "../../enums/measurementTypes";
import type { Measurement } from "../../models/measurement";
import type {
  MeasurementsBySensor,
  MeasurementsViewModel,
} from "../../models/measurementsBySensor";
import type { Sensor } from "../../models/sensor";
import {
  formatMeasurement,
  getMeasurementTypeDisplayName,
} from "../../utilities/measurementUtils";
import { stringSort } from "../../utilities/stringUtils";

export interface MeasurementsMapProps {
  model: MeasurementsViewModel | undefined;
  measurementTypes?: number[];
  minHeight?: number | null;
}

interface SensorMapItem {
  sensor: Sensor;
  latestMeasurements: Measurement[];
  primaryMeasurement: Measurement;
  temperatureMeasurement: Measurement | undefined;
}

const getSensorTitle = (sensor: Sensor) =>
  sensor.displayName ?? sensor.name ?? sensor.identifier;

const getFilteredLatestMeasurements = (
  measurementsBySensor: MeasurementsBySensor,
  measurementTypes?: number[],
) => {
  const latestMeasurements = Object.values(measurementsBySensor.latestValues);

  return latestMeasurements
    .filter((measurement) =>
      measurementTypes && measurementTypes.length > 0
        ? measurementTypes.includes(measurement.typeId)
        : true,
    )
    .sort((a, b) =>
      stringSort(
        getMeasurementTypeDisplayName(a.typeId as MeasurementTypes),
        getMeasurementTypeDisplayName(b.typeId as MeasurementTypes),
      ),
    );
};

const getTemperatureColor = (temperature?: number) => {
  if (temperature === undefined) {
    return {
      borderColor: "#546e7a",
      fillColor: "#78909c",
      tooltipBorderColor: "rgba(84, 110, 122, 0.3)",
    };
  }

  if (temperature <= -10) {
    return {
      borderColor: "#0d47a1",
      fillColor: "#1976d2",
      tooltipBorderColor: "rgba(13, 71, 161, 0.3)",
    };
  }

  if (temperature <= 0) {
    return {
      borderColor: "#1565c0",
      fillColor: "#42a5f5",
      tooltipBorderColor: "rgba(21, 101, 192, 0.3)",
    };
  }

  if (temperature <= 10) {
    return {
      borderColor: "#00897b",
      fillColor: "#26a69a",
      tooltipBorderColor: "rgba(0, 137, 123, 0.3)",
    };
  }

  if (temperature <= 20) {
    return {
      borderColor: "#7cb342",
      fillColor: "#9ccc65",
      tooltipBorderColor: "rgba(124, 179, 66, 0.3)",
    };
  }

  if (temperature <= 28) {
    return {
      borderColor: "#f9a825",
      fillColor: "#fbc02d",
      tooltipBorderColor: "rgba(249, 168, 37, 0.3)",
    };
  }

  return {
    borderColor: "#c62828",
    fillColor: "#ef5350",
    tooltipBorderColor: "rgba(198, 40, 40, 0.3)",
  };
};

const MapViewport: React.FC<{ sensors: SensorMapItem[] }> = ({ sensors }) => {
  const map = useMap();

  useEffect(() => {
    if (sensors.length === 0) {
      return;
    }

    if (sensors.length === 1) {
      map.setView(
        [sensors[0].sensor.latitude ?? 0, sensors[0].sensor.longitude ?? 0],
        13,
      );
      return;
    }

    const bounds = L.latLngBounds(
      sensors.map((item) => [
        item.sensor.latitude ?? 0,
        item.sensor.longitude ?? 0,
      ]),
    );

    map.fitBounds(bounds, {
      padding: [12, 12],
      maxZoom: 16,
    });
  }, [map, sensors]);

  return null;
};

export const MeasurementsMap: React.FC<MeasurementsMapProps> = ({
  model,
  measurementTypes,
  minHeight = 360,
}) => {
  const [hoveredSensorIdentifier, setHoveredSensorIdentifier] = useState<
    string | null
  >(null);

  const sensorsWithCoordinates = useMemo(() => {
    const availableSensors = model?.sensors ?? [];

    return availableSensors
      .map((sensor) => {
        const measurementsBySensor = model?.measurements.find(
          (item) => item.sensorIdentifier === sensor.identifier,
        );

        if (!measurementsBySensor) {
          return undefined;
        }

        const latestMeasurements = getFilteredLatestMeasurements(
          measurementsBySensor,
          measurementTypes,
        );

        if (latestMeasurements.length === 0) {
          return undefined;
        }

        if (sensor.latitude === undefined || sensor.longitude === undefined) {
          return undefined;
        }

        const temperatureMeasurement = latestMeasurements.find(
          (measurement) => measurement.typeId === MeasurementTypes.Temperature,
        );

        return {
          sensor,
          latestMeasurements,
          primaryMeasurement: temperatureMeasurement ?? latestMeasurements[0],
          temperatureMeasurement,
        };
      })
      .filter((item): item is SensorMapItem => item !== undefined)
      .sort((a, b) =>
        stringSort(getSensorTitle(a.sensor), getSensorTitle(b.sensor)),
      );
  }, [measurementTypes, model]);

  return (
    <Paper
      elevation={0}
      sx={{
        mt: 2,
        p: 1,
        width: "100%",
        height: "100%",
        overflow: "hidden",
        display: "flex",
        flexDirection: "column",
        borderRadius: 1,
        border: "1px solid",
        borderColor: "divider",
        backgroundColor: "background.paper",
      }}
    >
      {sensorsWithCoordinates.length === 0 ? (
        <Box
          sx={{
            minHeight: minHeight ?? undefined,
            flex: 1,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            border: "1px dashed",
            borderColor: "divider",
            borderRadius: 2,
            color: "text.secondary",
            px: 2,
            textAlign: "center",
          }}
        >
          No sensor locations available for the selected measurements.
        </Box>
      ) : (
        <Box
          sx={{
            minHeight: minHeight ?? undefined,
            flex: 1,
            height: "100%",
            borderRadius: 2,
            overflow: "hidden",
            border: "1px solid",
            borderColor: "divider",
            "& .leaflet-container": {
              minHeight: minHeight ?? undefined,
              height: "100%",
              width: "100%",
              fontFamily: "inherit",
            },
            "& .leaflet-tooltip.sensor-tooltip": {
              backgroundColor: "rgba(255, 255, 255, 0.95)",
              border: "1px solid rgba(0, 0, 0, 0.12)",
              borderRadius: "6px",
              boxShadow: "0 3px 8px rgba(0, 0, 0, 0.12)",
              color: "inherit",
              padding: "4px 6px",
              margin: 0,
            },
            "& .leaflet-tooltip.sensor-tooltip:before": {
              borderTopColor: "rgba(255, 255, 255, 0.95)",
            },
            "& .leaflet-tooltip.sensor-summary-tooltip": {
              backgroundColor: "rgba(255, 255, 255, 0.96)",
              borderRadius: "10px",
              boxShadow: "0 2px 6px rgba(0, 0, 0, 0.1)",
              color: "inherit",
              padding: "2px 6px",
              margin: 0,
              fontSize: "0.7rem",
              fontWeight: 700,
            },
            "& .leaflet-tooltip.sensor-summary-tooltip:before": {
              display: "none",
            },
          }}
        >
          <MapContainer
            center={[0, 0]}
            zoom={2}
            scrollWheelZoom
            style={{ height: "100%", width: "100%" }}
          >
            <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
            <MapViewport sensors={sensorsWithCoordinates} />
            {sensorsWithCoordinates.map((item) => {
              const markerColor = getTemperatureColor(
                item.temperatureMeasurement?.sensorValue,
              );

              return (
                <CircleMarker
                  key={item.sensor.identifier}
                  center={[item.sensor.latitude!, item.sensor.longitude!]}
                  radius={8}
                  eventHandlers={{
                    mouseover: () => {
                      setHoveredSensorIdentifier(item.sensor.identifier);
                    },
                    mouseout: () => {
                      setHoveredSensorIdentifier((current) =>
                        current === item.sensor.identifier ? null : current,
                      );
                    },
                  }}
                  pathOptions={{
                    color: markerColor.borderColor,
                    fillColor: markerColor.fillColor,
                    fillOpacity: 0.85,
                    weight: 2,
                  }}
                >
                  <Tooltip
                    permanent
                    direction="right"
                    offset={[12, 0]}
                    className={
                      hoveredSensorIdentifier === item.sensor.identifier
                        ? "sensor-tooltip"
                        : "sensor-summary-tooltip"
                    }
                  >
                    {hoveredSensorIdentifier === item.sensor.identifier ? (
                      <Box>
                        <Typography
                          variant="caption"
                          sx={{
                            display: "block",
                            fontWeight: 700,
                            lineHeight: 1.2,
                          }}
                        >
                          {getSensorTitle(item.sensor)}
                        </Typography>
                        {item.latestMeasurements.map((measurement) => (
                          <Typography
                            key={`${item.sensor.identifier}-${measurement.typeId}`}
                            variant="caption"
                            color="text.secondary"
                            sx={{ display: "block", lineHeight: 1.15 }}
                          >
                            {getMeasurementTypeDisplayName(
                              measurement.typeId as MeasurementTypes,
                            )}
                            {": "}
                            {formatMeasurement(measurement)}
                          </Typography>
                        ))}
                      </Box>
                    ) : (
                      <Box
                        component="span"
                        sx={{
                          display: "inline-block",
                          borderBottom: "2px solid",
                          borderColor: markerColor.tooltipBorderColor,
                          lineHeight: 1.1,
                        }}
                      >
                        {formatMeasurement(item.primaryMeasurement, true)}
                      </Box>
                    )}
                  </Tooltip>
                </CircleMarker>
              );
            })}
          </MapContainer>
        </Box>
      )}
    </Paper>
  );
};
