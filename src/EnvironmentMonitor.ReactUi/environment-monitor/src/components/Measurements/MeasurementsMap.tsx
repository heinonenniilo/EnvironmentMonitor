import { Box, Paper, Typography } from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import {
  CircleMarker,
  MapContainer,
  Popup,
  TileLayer,
  Tooltip,
  useMap,
  useMapEvents,
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
import { MultiSensorGraph } from "./MultiSensorGraph";

export interface MeasurementsMapProps {
  model: MeasurementsViewModel | undefined;
  measurementTypes?: number[];
  minHeight?: number | null;
  onHoveredSensorIdentifierChange?: (sensorIdentifier: string | null) => void;
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

const getTemperatureFillColor = (temperature?: number) => {
  if (temperature === undefined) {
    return "#78909c";
  }

  if (temperature <= -30) return "#3B0F70";
  if (temperature <= -25) return "#1F4E9E";
  if (temperature <= -20) return "#2C7FB8";
  if (temperature <= -15) return "#41B6C4";
  if (temperature <= -10) return "#7FD3E8";
  if (temperature <= -5) return "#BDEDF6";
  if (temperature <= 0) return "#F2F2F2";
  if (temperature <= 5) return "#FFF3A1";
  if (temperature <= 10) return "#FFD166";
  if (temperature <= 15) return "#FCA311";
  if (temperature <= 20) return "#F77F00";
  if (temperature <= 25) return "#E85D04";
  if (temperature <= 30) return "#D62828";

  return "#7F0000";
};

const getTemperatureColor = (temperature?: number) => {
  if (temperature === undefined) {
    return {
      borderColor: "#37474f",
      fillColor: "#78909c",
      tooltipBorderColor: "#37474f",
      textColor: "#102027",
    };
  }

  const fillColor = getTemperatureFillColor(temperature);
  let borderColor: string;
  const textColor = "#111111";

  if (temperature <= -30) {
    borderColor = "#220748";
  } else if (temperature <= -25) {
    borderColor = "#15386f";
  } else if (temperature <= -20) {
    borderColor = "#18577f";
  } else if (temperature <= -15) {
    borderColor = "#23838f";
  } else if (temperature <= -10) {
    borderColor = "#2f95ad";
  } else if (temperature <= -5) {
    borderColor = "#2d7f98";
  } else if (temperature <= 0) {
    borderColor = "#9e9e9e";
  } else if (temperature <= 5) {
    borderColor = "#caa500";
  } else if (temperature <= 10) {
    borderColor = "#cf9200";
  } else if (temperature <= 15) {
    borderColor = "#c27700";
  } else if (temperature <= 20) {
    borderColor = "#b85d00";
  } else if (temperature <= 25) {
    borderColor = "#a64100";
  } else if (temperature <= 30) {
    borderColor = "#8f1a1a";
  } else {
    borderColor = "#4d0000";
  }

  return {
    borderColor,
    fillColor,
    tooltipBorderColor: borderColor,
    textColor,
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

const MapInteractionHandler: React.FC<{ onMapClick: () => void }> = ({
  onMapClick,
}) => {
  useMapEvents({
    click: () => {
      onMapClick();
    },
  });

  return null;
};

interface SensorTooltipContentProps {
  item: SensorMapItem;
}

const SensorHoverTooltipContent: React.FC<SensorTooltipContentProps> = ({
  item,
}) => {
  return (
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
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{ display: "block", lineHeight: 1.15 }}
      >
        {formatMeasurement(item.primaryMeasurement)}
      </Typography>
    </Box>
  );
};

interface SensorPopupContentProps {
  item: SensorMapItem;
  model: MeasurementsViewModel | undefined;
}

const SensorPopupContent: React.FC<SensorPopupContentProps> = ({
  item,
  model,
}) => {
  return (
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
      <Box sx={{ width: 320, height: 200, mt: 0.5 }}>
        <MultiSensorGraph
          sensors={[item.sensor]}
          model={{
            measurements:
              model?.measurements.filter(
                (measurementBySensor) =>
                  measurementBySensor.sensorIdentifier ===
                  item.sensor.identifier,
              ) ?? [],
            sensors: [item.sensor],
          }}
          minHeight={180}
          hideInfo
          hideHeader
          hideUseAutoScale
          useAutoScale
        />
      </Box>
    </Box>
  );
};

export const MeasurementsMap: React.FC<MeasurementsMapProps> = ({
  model,
  measurementTypes,
  minHeight = 360,
  onHoveredSensorIdentifierChange,
}) => {
  const [hoveredSensorIdentifier, setHoveredSensorIdentifier] = useState<
    string | null
  >(null);
  const [selectedSensorIdentifier, setSelectedSensorIdentifier] = useState<
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

  useEffect(() => {
    onHoveredSensorIdentifierChange?.(
      selectedSensorIdentifier ?? hoveredSensorIdentifier,
    );
  }, [
    hoveredSensorIdentifier,
    onHoveredSensorIdentifierChange,
    selectedSensorIdentifier,
  ]);

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
            <MapInteractionHandler
              onMapClick={() => {
                setSelectedSensorIdentifier(null);
              }}
            />
            <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
            <MapViewport sensors={sensorsWithCoordinates} />
            {sensorsWithCoordinates.map((item) => {
              const markerColor = getTemperatureColor(
                item.temperatureMeasurement?.sensorValue,
              );
              const showHoverTooltip =
                hoveredSensorIdentifier === item.sensor.identifier &&
                selectedSensorIdentifier !== item.sensor.identifier;

              return (
                <CircleMarker
                  key={item.sensor.identifier}
                  center={[item.sensor.latitude!, item.sensor.longitude!]}
                  radius={9}
                  eventHandlers={{
                    mouseover: () => {
                      setHoveredSensorIdentifier(item.sensor.identifier);
                    },
                    mouseout: () => {
                      setHoveredSensorIdentifier((current) =>
                        current === item.sensor.identifier ? null : current,
                      );
                    },
                    click: () => {
                      setSelectedSensorIdentifier(item.sensor.identifier);
                    },
                    popupclose: () => {
                      setSelectedSensorIdentifier((current) =>
                        current === item.sensor.identifier ? null : current,
                      );
                    },
                  }}
                  pathOptions={{
                    color: markerColor.borderColor,
                    fillColor: markerColor.fillColor,
                    fillOpacity: 0.92,
                    weight: 3,
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
                    {showHoverTooltip ? (
                      <SensorHoverTooltipContent item={item} />
                    ) : (
                      <Box
                        component="span"
                        sx={{
                          display: "inline-block",
                          borderBottom: "2px solid",
                          borderColor: markerColor.tooltipBorderColor,
                          color: markerColor.textColor,
                          backgroundColor: "rgba(255,255,255,0.88)",
                          borderRadius: "6px",
                          px: 0.5,
                          py: "1px",
                          lineHeight: 1.1,
                          fontWeight: 700,
                        }}
                      >
                        {formatMeasurement(item.primaryMeasurement, true)}
                      </Box>
                    )}
                  </Tooltip>
                  <Popup minWidth={340}>
                    <SensorPopupContent item={item} model={model} />
                  </Popup>
                </CircleMarker>
              );
            })}
          </MapContainer>
        </Box>
      )}
    </Paper>
  );
};
