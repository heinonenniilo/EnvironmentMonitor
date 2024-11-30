import "chartjs-adapter-moment";
import { MeasurementTypes } from "../enums/temperatureTypes";
import { Sensor } from "../models/sensor";
import { Box, Typography } from "@mui/material";
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
  device?: Device;
  model: MeasurementsViewModel | undefined;
  hideInfo?: boolean;
}

export const MultiSensorGraph: React.FC<MultiSensorGraphProps> = ({
  sensors,
  model,
  device,
  hideInfo,
}) => {
  const validSensors = device
    ? sensors?.filter((s) => s.deviceId === device.id)
    : sensors;

  const isSensorValid = (sensorId: number) => {
    return validSensors?.some((s) => s.id === sensorId);
  };

  const getDatasets = () => {
    const returnValues: any[] = [];
    if (!model || model.measurements.length === 0) {
      return { datasets: returnValues };
    }
    model.measurements
      .filter((m) => isSensorValid(m.sensorId))
      .forEach((m) => {
        for (let item in MeasurementTypes) {
          let val = parseInt(MeasurementTypes[item]);
          if (m.measurements.some((m) => m.typeId === val)) {
            returnValues.push({
              label: getSensorLabel(m.sensorId, val as MeasurementTypes),
              data: m.measurements
                .filter((d) => d.typeId === val)
                .map((d) => {
                  return {
                    x: d.timestamp,
                    y: d.sensorValue,
                  };
                }),
            });
          }
        }
      });
    return {
      datasets: returnValues,
    };
  };

  const getInfoValues = () => {
    let returnArray: MeasurementInfo[] = [];
    model?.measurements
      .filter((m) => isSensorValid(m.sensorId))
      .forEach((m) => {
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
    if (!device) {
      return "Select a device";
    }
    return `${device?.name} / (${device?.id})`;
  };

  const getSensorLabel = (sensorId: number, typeId?: MeasurementTypes) => {
    const sensorName =
      sensors?.find((s) => s.id === sensorId)?.name ?? `${sensorId}`;

    if (!typeId) {
      return sensorName;
    } else {
      return `${sensorName} ${getMeasurementUnit(typeId)}`;
    }
  };

  const getMinScale = () => {
    let minScales =
      validSensors
        ?.filter((d) => d.scaleMin !== undefined)
        .map((d) => d.scaleMin ?? 0) ?? [];
    return validSensors?.some((d) => d.scaleMin !== undefined)
      ? Math.min(...minScales)
      : undefined;
  };

  const getMaxScale = () => {
    let maxScales =
      validSensors
        ?.filter((d) => d.scaleMax !== undefined)
        .map((d) => d.scaleMax ?? 0) ?? [];
    return validSensors?.some((d) => d.scaleMax !== undefined)
      ? Math.max(...maxScales)
      : undefined;
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
      <Box width="100%" mt={0} flexGrow={0}>
        <Typography align="left" gutterBottom>
          {getTitle()}
        </Typography>
      </Box>
      <Box
        flex={1}
        flexGrow={1}
        width={"100%"}
        display={"flex"}
        flexDirection={"column"}
        maxWidth={"100%"}
        maxHeight={"100%"}
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
            data={getDatasets()}
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
              },
            }}
          />
        </div>
      </Box>
      {!hideInfo ? (
        <Box width={"100%"} maxHeight={"300px"} overflow={"auto"}>
          <MeasurementsInfoTable infoRows={getInfoValues()} />
        </Box>
      ) : null}
    </Box>
  );
};
