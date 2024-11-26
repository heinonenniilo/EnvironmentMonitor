import { LineChart } from "@mui/x-charts";
import { MeasurementTypes } from "../enums/temperatureTypes";
import { Sensor } from "../models/sensor";
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { MeasurementsBySensor } from "../models/measurementsBySensor";
import { formatMeasurement } from "../utilities/measurementUtils";
import { Device } from "../models/device";

export interface MeasurementGraphProps {
  sensor: Sensor | undefined;
  device?: Device;
  model: MeasurementsBySensor | undefined;
}

export const MeasurementGraph: React.FC<MeasurementGraphProps> = ({
  sensor,
  model,
  device,
}) => {
  const getMeasurements = () => {
    if (!model) {
      return [];
    }
    const temp = model.measurements
      .filter((x) => x.typeId === MeasurementTypes.Temperature)
      .map((x) => {
        // Get humidity, assume same datetime
        const humidityRow = model.measurements.find(
          (m) =>
            m.timestamp === x.timestamp &&
            m.typeId === MeasurementTypes.Humidity
        );
        return {
          timestamp: new Date(x.timestamp),
          temperature: x.sensorValue,
          humidity: humidityRow?.sensorValue,
        };
      });
    return temp;
  };

  const hasHumidity = model?.measurements.some(
    (d) => d.typeId === MeasurementTypes.Humidity
  );

  const getMinMeasurement = (type: MeasurementTypes) => {
    if (model?.minValues !== undefined && model.minValues[type] !== undefined) {
      return formatMeasurement(model.minValues[type]);
    }
    return "-";
  };

  const getMaxMeasurement = (type: MeasurementTypes) => {
    if (model?.maxValues !== undefined && model.maxValues[type] !== undefined) {
      return formatMeasurement(model.maxValues[type]);
    }
    return "-";
  };

  const getLatestMeasurement = (type: MeasurementTypes) => {
    if (
      model?.latestValues !== undefined &&
      model.latestValues[type] !== undefined
    ) {
      return formatMeasurement(model.latestValues[type]);
    }
    return "-";
  };

  const getTitle = () => {
    let sensorInfo = `${sensor?.name} (${sensor?.sensorId})`;
    if (device) {
      sensorInfo += ` / ${device.name}`;
    }
    return sensorInfo;
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
        {sensor ? (
          <Typography align="left" gutterBottom>
            {getTitle()}
          </Typography>
        ) : null}
      </Box>
      <Box
        flex={1}
        flexGrow={1}
        height={"100%"}
        width={"100%"}
        display={"flex"}
        flexDirection={"column"}
        maxHeight={"800px"}
        minHeight={"400px"}
      >
        <LineChart
          dataset={getMeasurements()}
          xAxis={[{ dataKey: "timestamp", scaleType: "time" }]}
          yAxis={[
            {
              // dataKey: "temperature",
              label: "Measurement",
              min: sensor?.scaleMin,
              max: sensor?.scaleMax,
            },
          ]}
          series={
            hasHumidity
              ? [
                  {
                    dataKey: "temperature",
                    label: "Temperature °C",
                    showMark: false,
                  },
                  { dataKey: "humidity", label: "Humidity %", showMark: false },
                ]
              : [
                  {
                    dataKey: "temperature",
                    label: "Temperature °C",
                    showMark: false,
                  },
                ]
          }
        />
      </Box>
      <Box width="100%">
        <TableContainer>
          <Table size="small" sx={{ fontSize: 1 }}>
            <TableHead>
              <TableRow>
                <TableCell align="center">Min</TableCell>
                <TableCell align="center">Max</TableCell>
                <TableCell align="center">Latest</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              <TableRow>
                <TableCell align="center">
                  {getMinMeasurement(MeasurementTypes.Temperature)}
                </TableCell>
                <TableCell align="center">
                  {getMaxMeasurement(MeasurementTypes.Temperature)}
                </TableCell>
                <TableCell align="center">
                  {getLatestMeasurement(MeasurementTypes.Temperature)}
                </TableCell>
              </TableRow>
              {hasHumidity && (
                <TableRow>
                  <TableCell align="center">
                    {getMinMeasurement(MeasurementTypes.Humidity)}
                  </TableCell>
                  <TableCell align="center">
                    {getMaxMeasurement(MeasurementTypes.Humidity)}
                  </TableCell>
                  <TableCell align="center">
                    {getLatestMeasurement(MeasurementTypes.Humidity)}
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
      <div></div>
    </Box>
  );
};
