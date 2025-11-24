import "chartjs-adapter-moment";
import { MeasurementTypes } from "../enums/measurementTypes";
import { type Sensor } from "../models/sensor";
import {
  Box,
  Button,
  Checkbox,
  CircularProgress,
  FormControlLabel,
  Typography,
} from "@mui/material";
import { type MeasurementsViewModel } from "../models/measurementsBySensor";
import { type Device } from "../models/device";
import {
  Chart,
  Colors,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  TimeScale,
  Tooltip,
} from "chart.js";
import { Line } from "react-chartjs-2";
import {
  getDatasetLabel,
  getMeasurementUnit,
} from "../utilities/measurementUtils";
import {
  type MeasurementInfo,
  MeasurementsInfoTable,
} from "./MeasurementsInfoTable";
import { Link } from "react-router";
import { routes } from "../utilities/routes";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { stringSort } from "../utilities/stringUtils";
import { getColor } from "../utilities/graphUtils";
import zoomPlugin from "chartjs-plugin-zoom";
import { MeasurementsDialog } from "./MeasurementsDialog";
import type { Measurement } from "../models/measurement";

Chart.register(
  TimeScale,
  LinearScale,
  PointElement,
  LineElement,
  Tooltip,
  Legend,
  Colors,
  zoomPlugin
);

export interface MultiSensorGraphProps {
  sensors: Sensor[] | undefined;
  devices?: Device[];
  model: MeasurementsViewModel | undefined;
  hideInfo?: boolean;
  minHeight?: number;
  titleAsLink?: boolean;
  useAutoScale?: boolean;
  isLoading?: boolean;
  title?: string;
  useDynamicColors?: boolean;
  stepped?: boolean;
  zoomable?: boolean;
  hideUseAutoScale?: boolean;
  highlightPoints?: boolean;
  showMeasurementsOnDatasetClick?: boolean;
  onSetAutoScale?: (state: boolean) => void;
  onRefresh?: () => void;
}

interface GraphDataset {
  label: string;
  yAxisID: string;
  data: {
    x: Date;
    y: number;
  }[];
  id: number;
  measurementType: MeasurementTypes;
  borderColor?: string;
  backgroundColor?: string;
  stepped?: boolean;
  sensorIdentifier: string;
}

const dynamicColorLimit = 7;

export const MultiSensorGraph: React.FC<MultiSensorGraphProps> = ({
  sensors,
  model,
  devices,
  hideInfo,
  minHeight,
  titleAsLink,
  useAutoScale,
  onSetAutoScale,
  onRefresh,
  title,
  isLoading,
  useDynamicColors,
  stepped,
  zoomable,
  hideUseAutoScale,
  showMeasurementsOnDatasetClick,
  highlightPoints,
}) => {
  const singleDevice = devices && devices.length === 1 ? devices[0] : undefined;

  const [autoScale, setAutoScale] = useState(false);
  const [hiddenDatasetIds, setHiddenDatasetIds] = useState<number[]>([]);
  const [measurementsToShow, setMeasurementsToShow] = useState<Measurement[]>(
    []
  );
  const [dialogTitle, setDialogTitle] = useState("");
  const [highlightedDatasetLabel, setHighlightedDatasetLabel] = useState<
    string | null
  >(null);
  const chartRef = useRef<any>(null); // Types?

  useEffect(() => {
    if (useAutoScale !== undefined) {
      setAutoScale(useAutoScale);
    }
  }, [useAutoScale]);

  const getSensorLabel = useCallback(
    (sensorIdentifier: string, typeId?: MeasurementTypes) => {
      const matchingSensor = sensors?.find(
        (s) => s.identifier === sensorIdentifier
      );
      let sensorName = matchingSensor?.name ?? `${sensorIdentifier}`;
      const device =
        devices &&
        devices.length > 1 &&
        devices.find((d) => d.identifier === matchingSensor?.deviceIdentifier);
      if (device) {
        sensorName = device.displayName
          ? `${device.displayName}: ${sensorName}`
          : `${device.name}: ${sensorName}`;
      }

      return getDatasetLabel(sensorName, typeId as MeasurementTypes);
    },
    [sensors, devices]
  );

  const showMeasurementsInDialog = (
    sensorIdentifier: string,
    type?: MeasurementTypes
  ) => {
    const toShow = model?.measurements.find(
      (s) => s.sensorIdentifier === sensorIdentifier
    );
    if (toShow) {
      const matchingSensor = sensors?.find(
        (s) => s.identifier == toShow.sensorIdentifier
      );
      const matchingDevice = devices?.find(
        (d) => d.identifier === matchingSensor?.deviceIdentifier
      );
      setDialogTitle(
        matchingDevice
          ? `${matchingDevice?.displayName} / ${matchingSensor?.name} ${
              type ? getMeasurementUnit(type) : ""
            }`
          : `${matchingSensor?.name} ${type ? getMeasurementUnit(type) : ""}`
      );
      setMeasurementsToShow(
        toShow.measurements.filter((m) => m.typeId === type)
      );
    }
  };

  const handleResetZoom = () => {
    chartRef.current?.resetZoom();
  };

  const handleRowHover = (info: MeasurementInfo | null) => {
    setHighlightedDatasetLabel(info?.label ?? null);
  };

  const memoSets: GraphDataset[] = useMemo(() => {
    if (!model) return [];
    const returnValues: GraphDataset[] = [];
    let id = 0;

    for (const measurementsBySensor of model.measurements) {
      for (const item in MeasurementTypes) {
        const val = parseInt(MeasurementTypes[item]);
        if (measurementsBySensor.measurements.some((m) => m.typeId === val)) {
          const yAxisId = val === MeasurementTypes.Light ? "y1" : "y";

          returnValues.push({
            id: id++,
            label: getSensorLabel(measurementsBySensor.sensorIdentifier, val),
            yAxisID: yAxisId,
            measurementType: val,
            stepped: stepped,
            sensorIdentifier: measurementsBySensor.sensorIdentifier,
            data: measurementsBySensor.measurements
              .filter((d) => d.typeId === val)
              .map((d) => ({ x: d.timestamp, y: d.sensorValue })),
          });
        }
      }
    }

    return returnValues
      .sort((a, b) => stringSort(a.label, b.label))
      .map((s, idx) => {
        const isHighlighted = highlightedDatasetLabel === s.label;
        const color = getColor(idx);

        return {
          ...s,
          borderColor: color,
          backgroundColor: color,
          borderWidth: isHighlighted ? 5 : 2,
          id: idx,
        };
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [model, highlightedDatasetLabel]);

  const getInfoValues = () => {
    const returnArray: MeasurementInfo[] = [];
    model?.measurements.forEach((m) => {
      for (const item in MeasurementTypes) {
        const val = parseInt(MeasurementTypes[item]) as MeasurementTypes;

        if (m.minValues[val] !== undefined) {
          returnArray.push({
            min: m.minValues[val],
            max: m.maxValues[val],
            sensor: sensors?.find((s) => s.identifier === m.sensorIdentifier),
            latest: m.latestValues[val],
            label: getSensorLabel(m.sensorIdentifier, val),
            type: val,
          });
        }
      }
    });
    return returnArray.sort((a, b) => stringSort(a.label, b.label));
  };

  const getTitle = () => {
    if (title) {
      return title;
    }
    if (!singleDevice) {
      if (!devices || devices.length === 0) {
        return "Select a device";
      } else {
        return "";
      }
    }

    if (singleDevice.displayName) {
      return singleDevice.displayName;
    }

    return `${singleDevice?.name} / (${singleDevice?.identifier})`;
  };

  const getMinScale = () => {
    if (autoScale) {
      return undefined;
    }
    const minScales =
      sensors
        ?.filter((d) => d.scaleMin !== undefined)
        .map((d) => d.scaleMin ?? 0) ?? [];
    return sensors?.some((d) => d.scaleMin !== undefined)
      ? Math.min(...minScales)
      : undefined;
  };

  const getMaxScale = () => {
    if (autoScale) {
      return undefined;
    }
    const maxScales =
      sensors
        ?.filter((d) => d.scaleMax !== undefined)
        .map((d) => d.scaleMax ?? 0) ?? [];
    return sensors?.some((d) => d.scaleMax !== undefined)
      ? Math.max(...maxScales)
      : undefined;
  };

  const hasLightAxis = () => {
    const hasLightAxis = memoSets
      .filter((d) => !hiddenDatasetIds.some((h) => h === d.id))
      .some((d) => d.measurementType === MeasurementTypes.Light);
    return hasLightAxis;
  };

  const isTouchDevice = () => {
    return window.matchMedia("(pointer: coarse)").matches;
  };

  return (
    <Box
      display="flex"
      flexDirection="column"
      alignItems="center"
      flex={1}
      flexGrow={1}
      sx={{ maxHeight: "100%", width: "100%" }}
      position={"relative"}
    >
      <MeasurementsDialog
        isOpen={measurementsToShow.length > 0}
        measurements={measurementsToShow}
        onClose={() => {
          setMeasurementsToShow([]);
        }}
        title={dialogTitle}
      />
      {isLoading && (
        <Box
          position="absolute"
          top={0}
          left={0}
          width="100%"
          height="100%"
          display="flex"
          justifyContent="center"
          alignItems="center"
          sx={{
            backgroundColor: "rgba(255,255,255,0.5)",
            zIndex: 2,
          }}
        >
          <CircularProgress />
        </Box>
      )}
      <Box
        width="100%"
        mt={0}
        flexGrow={0}
        flexDirection="row"
        display="flex"
        alignItems="center" // Align children vertically
      >
        {titleAsLink ? (
          <Link to={`${routes.measurements}/${singleDevice?.identifier}`}>
            <Typography align="left" gutterBottom>
              {getTitle()}
            </Typography>
          </Link>
        ) : (
          <Typography align="left" gutterBottom>
            {getTitle()}
          </Typography>
        )}
        {!hideUseAutoScale && (
          <FormControlLabel
            sx={{ marginLeft: 2 }}
            control={
              <Checkbox
                checked={autoScale}
                onChange={(_e, c) => {
                  setAutoScale(c);
                  if (onSetAutoScale) {
                    onSetAutoScale(c);
                  }
                }}
                inputProps={{ "aria-label": "controlled checkbox" }}
              />
            }
            label="Auto scale"
            componentsProps={{
              typography: { fontSize: "14px" }, // Adjust font size
            }}
          />
        )}
        {zoomable || onRefresh !== undefined ? (
          <Box
            sx={{
              display: "flex",
              flexDirection: "row",
              marginLeft: "auto",
              gap: 1,
            }}
          >
            {zoomable && (
              <Button
                variant="outlined"
                onClick={() => {
                  handleResetZoom();
                }}
                size="small"
              >
                Reset zoom
              </Button>
            )}
            {onRefresh && (
              <Button variant="outlined" onClick={onRefresh} size="small">
                Refresh
              </Button>
            )}
          </Box>
        ) : null}
      </Box>
      <Box
        flex={1}
        flexGrow={1}
        width={"100%"}
        display={"flex"}
        flexDirection={"column"}
        maxWidth={"100%"}
        maxHeight={"100%"}
        minHeight={minHeight}
      >
        <div
          style={{
            position: "relative",
            margin: "auto",
            width: "100%",
            height: "100%",
            // minHeight: "400px",
            flexGrow: 1,
          }}
        >
          <Line
            data={{ datasets: memoSets }}
            height={"auto"}
            ref={chartRef}
            options={{
              maintainAspectRatio: false,
              plugins: {
                title: {
                  text: "Chart.js Time Scale",
                  display: true,
                },
                colors:
                  useDynamicColors || memoSets.length > dynamicColorLimit
                    ? undefined
                    : {
                        forceOverride: true,
                      },
                legend: {
                  onClick: (_event, legendItem, legend) => {
                    if (legendItem.datasetIndex === undefined) {
                      return;
                    }
                    if (showMeasurementsOnDatasetClick) {
                      if (memoSets.length > legendItem.datasetIndex) {
                        const matchingDataset =
                          memoSets[legendItem.datasetIndex];
                        showMeasurementsInDialog(
                          matchingDataset.sensorIdentifier,
                          matchingDataset.measurementType
                        );
                      }
                      return;
                    }

                    if (!legendItem.hidden) {
                      legend.chart.hide(legendItem.datasetIndex);
                      if (legendItem.datasetIndex !== undefined) {
                        const datasetIndex = legendItem.datasetIndex;
                        setHiddenDatasetIds((prev) => [...prev, datasetIndex]);
                      }
                      legend.chart.update("hide");
                    } else {
                      legend.chart.show(legendItem.datasetIndex);
                      setHiddenDatasetIds(
                        hiddenDatasetIds.filter(
                          (d) => d !== legendItem.datasetIndex
                        )
                      );
                      legend.chart.update("show");
                    }
                  },
                  onHover: (event) => {
                    (event.native?.target as any).style.cursor = "pointer";
                  },
                  onLeave: (event) => {
                    (event.native?.target as any).style.cursor = "default";
                  },
                },
                zoom: zoomable
                  ? {
                      zoom: {
                        drag: {
                          enabled: true,
                          borderColor: "rgba(54,162,235,0.5)",
                          borderWidth: 1,
                          backgroundColor: "rgba(54,162,235,0.15)",
                        },
                        pinch: { enabled: true },
                        mode: "x",
                        wheel: { enabled: true },
                      },
                      pan: {
                        enabled: isTouchDevice(),
                        mode: "x",
                      },
                    }
                  : undefined,
              },
              elements: {
                point: {
                  radius: highlightPoints ? 2 : 0,
                },
              },
              responsive: true,
              scales: {
                x: {
                  type: "time",
                  time: {
                    //tooltipFormat: "DD T",
                    unit: "hour",
                    displayFormats: {
                      hour: "HH:mm",
                    },
                  },
                  ticks: {
                    major: {
                      enabled: true,
                    },
                    font: (context) => {
                      if (context.tick && context.tick.major) {
                        return {
                          weight: "bold",
                        };
                      }
                    },
                  },
                },
                y: {
                  max: getMaxScale(),
                  min: getMinScale(),
                },
                y1: {
                  max: undefined,
                  min: undefined,
                  display: hasLightAxis(),
                  position: "right",
                  ticks: {
                    callback: (value) => `${value} lx`,
                  },
                  grid: {
                    drawOnChartArea: false,
                  },
                },
              },
            }}
          />
        </div>
      </Box>
      {!hideInfo ? (
        <Box width={"100%"} maxHeight={"200px"} overflow={"auto"}>
          <MeasurementsInfoTable
            onClick={(row) => {
              if (!row.sensor) {
                return;
              }
              showMeasurementsInDialog(row.sensor.identifier, row.type);
            }}
            onHover={handleRowHover}
            infoRows={getInfoValues()}
          />
        </Box>
      ) : null}
    </Box>
  );
};
