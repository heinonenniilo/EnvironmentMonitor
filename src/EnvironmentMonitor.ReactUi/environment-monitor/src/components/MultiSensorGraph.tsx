import "chartjs-adapter-moment";
import { MeasurementTypes } from "../enums/measurementTypes";
import { type Sensor } from "../models/sensor";
import { Box, Dialog, DialogContent, DialogTitle } from "@mui/material";
import { type MeasurementsViewModel } from "../models/measurementsBySensor";
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
import {
  getDatasetLabel,
  getMeasurementUnit,
} from "../utilities/measurementUtils";
import {
  type MeasurementInfo,
  MeasurementsInfoTable,
} from "./MeasurementsInfoTable";
import { routes } from "../utilities/routes";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { stringSort } from "../utilities/stringUtils";
import { getColor } from "../utilities/graphUtils";
import zoomPlugin from "chartjs-plugin-zoom";
import { MeasurementsDialog } from "./MeasurementsDialog";
import type { Measurement } from "../models/measurement";
import type { Entity } from "../models/entity";
import { LoadingOverlay } from "../framework/LoadingOverlay";
import { GraphHeader } from "./GraphHeader";
import { LineGraph } from "../framework/LineGraph";
import type { GraphDataset } from "../models/GraphDataset";

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
  entities?: Entity[];
  model: MeasurementsViewModel | undefined;
  hideInfo?: boolean;
  minHeight?: number;
  titleAsLink?: boolean;
  linkToLocationMeasurements?: boolean;
  useAutoScale?: boolean;
  isLoading?: boolean;
  title?: string;
  useDynamicColors?: boolean;
  stepped?: boolean;
  zoomable?: boolean;
  hideUseAutoScale?: boolean;
  highlightPoints?: boolean;
  showMeasurementsOnDatasetClick?: boolean;
  isFullScreen?: boolean;
  enableHighlightOnRowHover?: boolean;
  showFullScreenIcon?: boolean;
  onSetAutoScale?: (state: boolean) => void;
  onRefresh?: () => void;
  onSetFullScreen?: (state: boolean) => void;
}

const dynamicColorLimit = 7;

export const MultiSensorGraph: React.FC<MultiSensorGraphProps> = ({
  sensors,
  model,
  entities,
  hideInfo,
  minHeight,
  titleAsLink,
  linkToLocationMeasurements,
  useAutoScale,
  title,
  isLoading,
  useDynamicColors,
  stepped,
  zoomable,
  hideUseAutoScale,
  showMeasurementsOnDatasetClick,
  highlightPoints,
  enableHighlightOnRowHover,
  showFullScreenIcon,
  isFullScreen: isFullScreenProp,
  onSetFullScreen,
  onSetAutoScale,
  onRefresh,
}) => {
  const singleDevice =
    entities && entities.length === 1 ? entities[0] : undefined;

  const [autoScale, setAutoScale] = useState(false);
  const [hiddenDatasetIds, setHiddenDatasetIds] = useState<number[]>([]);
  const [measurementsToShow, setMeasurementsToShow] = useState<Measurement[]>(
    []
  );
  const [dialogTitle, setDialogTitle] = useState("");
  const [highlightedDatasetLabel, setHighlightedDatasetLabel] = useState<
    string | null
  >(null);
  const [isFullScreen, setIsFullScreen] = useState(false);
  const chartRef = useRef<any>(null); // Types?

  useEffect(() => {
    if (useAutoScale !== undefined) {
      setAutoScale(useAutoScale);
    }
  }, [useAutoScale]);

  useEffect(() => {
    if (isFullScreenProp !== undefined) {
      setIsFullScreen(isFullScreenProp);
    }
  }, [isFullScreenProp]);

  const handleSetAutoScale = (state: boolean) => {
    setAutoScale(state);
    if (onSetAutoScale) {
      onSetAutoScale(state);
    }
  };

  const handleSetFullScreen = (state: boolean) => {
    if (onSetFullScreen) {
      onSetFullScreen(state);
    } else {
      setIsFullScreen(state);
    }
  };

  const getSensorLabel = useCallback(
    (sensorIdentifier: string, typeId?: MeasurementTypes) => {
      const matchingSensor = sensors?.find(
        (s) => s.identifier === sensorIdentifier
      );
      let sensorName = matchingSensor?.name ?? `${sensorIdentifier}`;
      const device =
        entities &&
        entities.length > 1 &&
        entities.find((d) => d.identifier === matchingSensor?.parentIdentifier);
      if (device) {
        sensorName = device.displayName
          ? `${device.displayName}: ${sensorName}`
          : `${device.name}: ${sensorName}`;
      }

      return getDatasetLabel(sensorName, typeId as MeasurementTypes);
    },
    [sensors, entities]
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
      const matchingEntity = entities?.find(
        (d) => d.identifier === matchingSensor?.parentIdentifier
      );
      setDialogTitle(
        matchingEntity
          ? `${matchingEntity?.displayName} / ${matchingSensor?.name} ${
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
    if (info) {
      const datasetIndex = memoSets.findIndex((ds) => ds.label === info.label);
      if (datasetIndex !== -1 && !hiddenDatasetIds.includes(datasetIndex)) {
        setHighlightedDatasetLabel(info.label);
      }
    } else {
      setHighlightedDatasetLabel(null);
    }
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
          opacity: !highlightedDatasetLabel ? 1 : isHighlighted ? 1 : 0.3,
          borderWidth: !highlightedDatasetLabel ? 2 : isHighlighted ? 4 : 1,
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
      if (!entities || entities.length === 0) {
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
          handleRowHover(null);
        }}
        title={dialogTitle}
      />
      <Dialog
        open={isFullScreen}
        onClose={() => handleSetFullScreen(false)}
        fullWidth
        fullScreen
        sx={{ padding: 2 }}
      >
        <DialogTitle>
          <GraphHeader
            title={getTitle()}
            showControls={true}
            hideUseAutoScale={hideUseAutoScale}
            autoScale={autoScale}
            onSetAutoScale={handleSetAutoScale}
            onRefresh={onRefresh}
            onClose={() => handleSetFullScreen(false)}
          />
        </DialogTitle>
        <DialogContent>
          <LoadingOverlay isLoading={isLoading ?? false} />
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
            <LineGraph
              datasets={memoSets}
              chartRef={chartRef}
              useDynamicColors={useDynamicColors}
              dynamicColorLimit={dynamicColorLimit}
              zoomable={zoomable}
              highlightPoints={highlightPoints}
              yAxisMax={getMaxScale()}
              yAxisMin={getMinScale()}
              hasSecondaryAxis={hasLightAxis()}
              showMeasurementsOnDatasetClick={showMeasurementsOnDatasetClick}
              enableHighlightOnRowHover={enableHighlightOnRowHover}
              onLegendClick={(_datasetIndex, dataset) => {
                showMeasurementsInDialog(
                  dataset.sensorIdentifier,
                  dataset.measurementType
                );
              }}
              onLegendHover={(datasetLabel) => {
                setHighlightedDatasetLabel(datasetLabel);
              }}
              onLegendLeave={() => {
                setHighlightedDatasetLabel(null);
              }}
              onDatasetToggle={(datasetIndex, hidden) => {
                if (hidden) {
                  setHiddenDatasetIds((prev) => [...prev, datasetIndex]);
                } else {
                  setHiddenDatasetIds(
                    hiddenDatasetIds.filter((d) => d !== datasetIndex)
                  );
                }
              }}
            />
          </div>
        </DialogContent>
      </Dialog>
      <LoadingOverlay isLoading={isLoading ?? false} />
      <GraphHeader
        title={getTitle()}
        titleAsLink={titleAsLink}
        linkTo={
          linkToLocationMeasurements
            ? `${routes.locationMeasurements}/${singleDevice?.identifier}`
            : `${routes.measurements}/${singleDevice?.identifier}`
        }
        zoomable={zoomable}
        hideUseAutoScale={hideUseAutoScale}
        autoScale={autoScale}
        onResetZoom={handleResetZoom}
        onFullScreen={
          showFullScreenIcon ? () => handleSetFullScreen(true) : undefined
        }
        onSetAutoScale={handleSetAutoScale}
        onRefresh={onRefresh}
        showControls={
          zoomable ||
          onRefresh !== undefined ||
          !hideUseAutoScale ||
          showFullScreenIcon ||
          false
        }
      />
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
          <LineGraph
            datasets={memoSets}
            chartRef={chartRef}
            useDynamicColors={useDynamicColors}
            dynamicColorLimit={dynamicColorLimit}
            zoomable={zoomable}
            highlightPoints={highlightPoints}
            yAxisMax={getMaxScale()}
            yAxisMin={getMinScale()}
            hasSecondaryAxis={hasLightAxis()}
            showMeasurementsOnDatasetClick={showMeasurementsOnDatasetClick}
            enableHighlightOnRowHover={enableHighlightOnRowHover}
            onLegendClick={(_datasetIndex, dataset) => {
              showMeasurementsInDialog(
                dataset.sensorIdentifier,
                dataset.measurementType
              );
            }}
            onLegendHover={(datasetLabel) => {
              setHighlightedDatasetLabel(datasetLabel);
            }}
            onLegendLeave={() => {
              setHighlightedDatasetLabel(null);
            }}
            onDatasetToggle={(datasetIndex, hidden) => {
              if (hidden) {
                setHiddenDatasetIds((prev) => [...prev, datasetIndex]);
              } else {
                setHiddenDatasetIds(
                  hiddenDatasetIds.filter((d) => d !== datasetIndex)
                );
              }
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
              if (enableHighlightOnRowHover) {
                handleRowHover(row);
              }
              showMeasurementsInDialog(row.sensor.identifier, row.type);
            }}
            onHover={
              enableHighlightOnRowHover
                ? measurementsToShow.length === 0
                  ? handleRowHover
                  : undefined
                : undefined
            }
            infoRows={getInfoValues()}
          />
        </Box>
      ) : null}
    </Box>
  );
};
