import "chartjs-adapter-moment";
import { MeasurementTypes } from "../enums/measurementTypes";
import { Sensor } from "../models/sensor";
import { Box, Checkbox, FormControlLabel, Typography } from "@mui/material";
import { MeasurementsViewModel } from "../models/measurementsBySensor";
import { Device } from "../models/device";
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
import { getMeasurementUnit } from "../utilities/measurementUtils";
import {
  MeasurementInfo,
  MeasurementsInfoTable,
} from "./MeasurementsInfoTable";
import { Link } from "react-router";
import { routes } from "../utilities/routes";
import { useCallback, useEffect, useState } from "react";

Chart.register(
  TimeScale,
  LinearScale,
  PointElement,
  LineElement,
  Tooltip,
  Legend,
  Colors
);

export interface MultiSensorGraphProps {
  sensors: Sensor[] | undefined;
  devices?: Device[];
  model: MeasurementsViewModel | undefined;
  hideInfo?: boolean;
  minHeight?: number;
  titleAsLink?: boolean;
  useAutoScale?: boolean;
  onSetAutoScale?: (state: boolean) => void;
}

interface GraphDataset {
  label: string;
  yAxisID: string;
  data: {
    x: Date;
    y: number;
  }[];
  id: number;
  visible: boolean;
  measurementType: MeasurementTypes;
}

export const MultiSensorGraph: React.FC<MultiSensorGraphProps> = ({
  sensors,
  model,
  devices,
  hideInfo,
  minHeight,
  titleAsLink,
  useAutoScale,
  onSetAutoScale,
}) => {
  const singleDevice = devices && devices.length === 1 ? devices[0] : undefined;

  const [autoScale, setAutoScale] = useState(false);
  const [dataSets, setDataSets] = useState<GraphDataset[]>([]);

  useEffect(() => {
    if (useAutoScale !== undefined) {
      setAutoScale(useAutoScale);
    }
  }, [useAutoScale]);

  const getSensorLabel = useCallback(
    (sensorId: number, typeId?: MeasurementTypes) => {
      const matchingSensor = sensors?.find((s) => s.id === sensorId);
      let sensorName = matchingSensor?.name ?? `${sensorId}`;
      const device =
        devices &&
        devices.length > 1 &&
        devices.find((d) => d.id === matchingSensor?.deviceId);
      if (device) {
        sensorName = `${device.name}: ${sensorName}`;
      }

      if (!typeId) {
        return sensorName;
      } else {
        return `${sensorName} (${getMeasurementUnit(typeId)})`;
      }
    },
    [sensors, devices]
  );

  useEffect(() => {
    if (!model) {
      setDataSets([]);
      return;
    }
    const returnValues: GraphDataset[] = [];
    let id: number = 0;
    model.measurements.forEach((measurementsBySensor) => {
      for (let item in MeasurementTypes) {
        let val = parseInt(MeasurementTypes[item]);
        if (measurementsBySensor.measurements.some((m) => m.typeId === val)) {
          const yAxisId =
            (val as MeasurementTypes) === MeasurementTypes.Light ? "y1" : "y";
          returnValues.push({
            id: id,
            visible: true,
            label: getSensorLabel(
              measurementsBySensor.sensorId,
              val as MeasurementTypes
            ),
            yAxisID: yAxisId,
            measurementType: val as MeasurementTypes,
            data: measurementsBySensor.measurements
              .filter((d) => d.typeId === val)
              .map((d) => {
                return {
                  x: d.timestamp,
                  y: d.sensorValue,
                };
              }),
          });
          id++;
        }
      }
    });

    setDataSets(returnValues);
  }, [model, getSensorLabel]);

  const getInfoValues = () => {
    let returnArray: MeasurementInfo[] = [];
    model?.measurements.forEach((m) => {
      for (let item in MeasurementTypes) {
        let val = parseInt(MeasurementTypes[item]) as MeasurementTypes;
        if (m.minValues[val] !== undefined) {
          returnArray.push({
            min: m.minValues[val],
            max: m.maxValues[val],
            latest: m.latestValues[val],
            label: getSensorLabel(m.sensorId, val),
          });
        }
      }
    });
    return returnArray;
  };

  const getTitle = () => {
    if (!singleDevice) {
      if (!devices || devices.length === 0) {
        return "Select a device";
      } else {
        return "";
      }
    }
    return `${singleDevice?.name} / (${singleDevice?.id})`;
  };

  const getMinScale = () => {
    if (autoScale) {
      return undefined;
    }
    let minScales =
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
    let maxScales =
      sensors
        ?.filter((d) => d.scaleMax !== undefined)
        .map((d) => d.scaleMax ?? 0) ?? [];
    return sensors?.some((d) => d.scaleMax !== undefined)
      ? Math.max(...maxScales)
      : undefined;
  };

  const hasLightAxis = () => {
    const hasLightAxis = dataSets
      .filter((d) => d.visible)
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
    >
      <Box
        width="100%"
        mt={0}
        flexGrow={0}
        flexDirection="row"
        display="flex"
        alignItems="center" // Align children vertically
      >
        {titleAsLink ? (
          <Link to={`${routes.measurements}/${singleDevice?.deviceIdentifier}`}>
            <Typography align="left" gutterBottom>
              {getTitle()}
            </Typography>
          </Link>
        ) : (
          <Typography align="left" gutterBottom>
            {getTitle()}
          </Typography>
        )}
        <FormControlLabel
          sx={{ marginLeft: 2 }}
          control={
            <Checkbox
              checked={autoScale}
              onChange={(e, c) => {
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
            data={{ datasets: dataSets }}
            height={"auto"}
            options={{
              maintainAspectRatio: false,
              plugins: {
                title: {
                  text: "Chart.js Time Scale",
                  display: true,
                },
                colors: {
                  forceOverride: true,
                },
                legend: {
                  onClick: (event, legendItem, legend) => {
                    if (legendItem.datasetIndex !== undefined) {
                      if (!legendItem.hidden) {
                        legend.chart.hide(legendItem.datasetIndex);
                        setDataSets((prev) =>
                          prev.map((prevDataSet) =>
                            prevDataSet.id === legendItem.datasetIndex
                              ? { ...prevDataSet, visible: false }
                              : prevDataSet
                          )
                        );
                        legend.chart.update("hide");
                      } else {
                        legend.chart.show(legendItem.datasetIndex);
                        setDataSets((prev) =>
                          prev.map((prevDataSet) =>
                            prevDataSet.id === legendItem.datasetIndex
                              ? { ...prevDataSet, visible: true }
                              : prevDataSet
                          )
                        );
                        legend.chart.update("show");
                      }
                    }
                  },
                },
              },
              elements: {
                point: {
                  radius: 0,
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
          <MeasurementsInfoTable infoRows={getInfoValues()} />
        </Box>
      ) : null}
    </Box>
  );
};
