import { Line } from "react-chartjs-2";
import type { GraphDataset } from "../models/GraphDataset";

export interface LineGraphProps {
  datasets: GraphDataset[];
  chartRef?: any;
  useDynamicColors?: boolean;
  dynamicColorLimit?: number;
  zoomable?: boolean;
  highlightPoints?: boolean;
  yAxisMax?: number;
  yAxisMin?: number;
  hasSecondaryAxis?: boolean;
  showMeasurementsOnDatasetClick?: boolean;
  enableHighlightOnRowHover?: boolean;
  onLegendClick?: (datasetIndex: number, dataset: any) => void;
  onLegendHover?: (datasetLabel: string) => void;
  onLegendLeave?: () => void;
  onDatasetToggle?: (datasetIndex: number, hidden: boolean) => void;
}

const isTouchDevice = () => {
  return window.matchMedia("(pointer: coarse)").matches;
};

export const LineGraph: React.FC<LineGraphProps> = ({
  datasets,
  chartRef,
  useDynamicColors = false,
  dynamicColorLimit = 7,
  zoomable = false,
  highlightPoints = false,
  yAxisMax,
  yAxisMin,
  hasSecondaryAxis = false,
  showMeasurementsOnDatasetClick = false,
  enableHighlightOnRowHover = false,
  onLegendClick,
  onLegendHover,
  onLegendLeave,
  onDatasetToggle,
}) => {
  const chartOptions = {
    maintainAspectRatio: false,
    plugins: {
      title: {
        text: "Chart.js Time Scale",
        display: true,
      },
      colors:
        useDynamicColors || datasets.length > dynamicColorLimit
          ? undefined
          : {
              forceOverride: true,
            },
      legend: {
        onClick: (_event: any, legendItem: any, legend: any) => {
          if (legendItem.datasetIndex === undefined) {
            return;
          }

          if (showMeasurementsOnDatasetClick && onLegendClick) {
            if (datasets.length > legendItem.datasetIndex) {
              const matchingDataset = datasets[legendItem.datasetIndex];
              onLegendClick(legendItem.datasetIndex, matchingDataset);
            }
            return;
          }

          if (!legendItem.hidden) {
            legend.chart.hide(legendItem.datasetIndex);
            if (onDatasetToggle) {
              onDatasetToggle(legendItem.datasetIndex, true);
            }
            legend.chart.update("hide");
          } else {
            legend.chart.show(legendItem.datasetIndex);
            if (onDatasetToggle) {
              onDatasetToggle(legendItem.datasetIndex, false);
            }
            legend.chart.update("show");
          }
        },
        onHover: (event: any, legendItem: any) => {
          (event.native?.target as any).style.cursor = "pointer";
          if (
            enableHighlightOnRowHover &&
            !isTouchDevice() &&
            legendItem.datasetIndex !== undefined &&
            !legendItem.hidden &&
            onLegendHover
          ) {
            const dataset = datasets[legendItem.datasetIndex];
            if (dataset) {
              onLegendHover(dataset.label);
            }
          }
        },
        onLeave: (event: any) => {
          (event.native?.target as any).style.cursor = "default";
          if (enableHighlightOnRowHover && !isTouchDevice() && onLegendLeave) {
            onLegendLeave();
          }
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
              mode: "x" as const,
              wheel: { enabled: true },
            },
            pan: {
              enabled: isTouchDevice(),
              mode: "x" as const,
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
        type: "time" as const,
        time: {
          unit: "hour" as const,
          displayFormats: {
            hour: "HH:mm",
          },
        },
        ticks: {
          major: {
            enabled: true,
          },
          font: (context: any) => {
            if (context.tick && context.tick.major) {
              return {
                weight: "bold" as const,
              };
            }
          },
        },
      },
      y: {
        max: yAxisMax,
        min: yAxisMin,
      },
      y1: {
        max: undefined,
        min: undefined,
        display: hasSecondaryAxis,
        position: "right" as const,
        ticks: {
          callback: (value: any) => `${value} lx`,
        },
        grid: {
          drawOnChartArea: false,
        },
      },
    },
  };

  return (
    <Line
      data={{ datasets }}
      height={"auto"}
      ref={chartRef}
      options={chartOptions}
    />
  );
};
