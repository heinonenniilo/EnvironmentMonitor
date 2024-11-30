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
import {
  formatMeasurement,
  getMeasurementUnit,
} from "../utilities/measurementUtils";

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
        let label = "";
        const sensor = validSensors?.find((s) => s.id === m.sensorId);
        for (let item in MeasurementTypes) {
          let val = parseInt(MeasurementTypes[item]);
          console.log(val);
          if (m.measurements.some((m) => m.typeId === val)) {
            label = `${sensor?.name ?? m.sensorId} ${getMeasurementUnit(
              val as MeasurementTypes
            )}`;
            returnValues.push({
              label: label,
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
    let returnArray: any[] = [];

    model?.measurements.forEach((m) => {
      for (let item in MeasurementTypes) {
        if (!validSensors?.some((s) => s.id === m.sensorId)) {
          continue;
        }
        let val = MeasurementTypes[item];
        let g = parseInt(val);
        if (m.minValues[g as MeasurementTypes] !== undefined) {
          returnArray.push({
            minValues: m.minValues[g as MeasurementTypes],
            maxValues: m.maxValues[g as MeasurementTypes],
            latestValues: m.latestValues[g as MeasurementTypes],
            label: getSensorLabel(m.sensorId),
          });
        }
      }
    });
    return returnArray;
  };

  const getTitle = () => {
    return `${device?.name} / (${device?.id})`;
  };

  const getSensorLabel = (sensorId: number) => {
    return sensors?.find((s) => s.id === sensorId)?.name ?? `${sensorId}`;
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
      sx={{ height: "100%", width: "100%", maxHeight: "1000px" }}
    >
      <Box width="100%" mt={0} flexGrow={0}>
        <Typography align="left" gutterBottom>
          {getTitle()}
        </Typography>
      </Box>
      <Box
        flex={1}
        flexGrow={1}
        height={"100%"}
        width={"100%"}
        display={"flex"}
        flexDirection={"column"}
        maxHeight={"800px"}
        maxWidth={"100%"}
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
                },
                title: {
                  display: true,
                  text: "Date",
                },
              },
              y: {
                max: getMaxScale(),
                min: getMinScale(),
              },
            },
          }}
        />
      </Box>
      {!hideInfo ? (
        <Box width="100%">
          <Box
            display="flex"
            justifyContent="space-between"
            alignItems="center"
            sx={{
              fontWeight: "bold",
              borderBottom: "1px solid #ddd",
              padding: "4px 0",
            }}
          >
            <Box flex={1} textAlign="center">
              Censor
            </Box>
            <Box flex={1} textAlign="center">
              Min
            </Box>
            <Box flex={1} textAlign="center">
              Max
            </Box>
            <Box flex={1} textAlign="center">
              Latest
            </Box>
          </Box>
          {getInfoValues()?.map(
            ({ label, minValues, maxValues, latestValues }) => (
              <Box
                key={label}
                display="flex"
                justifyContent="space-between"
                alignItems="center"
                sx={{
                  borderBottom: "1px solid #ddd",
                  padding: "4px 0",
                  fontSize: 14,
                }}
              >
                <Box flex={1} textAlign="center">
                  {label}
                </Box>
                <Box flex={1} textAlign="center">
                  {formatMeasurement(minValues)}
                </Box>
                <Box flex={1} textAlign="center">
                  {formatMeasurement(maxValues)}
                </Box>
                <Box flex={1} textAlign="center">
                  {formatMeasurement(latestValues)}
                </Box>
              </Box>
            )
          )}
        </Box>
      ) : null}
    </Box>
  );
};
