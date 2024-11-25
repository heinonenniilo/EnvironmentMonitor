import { LineChart } from "@mui/x-charts";
import { MeasurementTypes } from "../enums/temperatureTypes";
import { Measurement } from "../models/measurement";
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

export interface MeasurementGraphProps {
  // devices: Device[];
  sensor: Sensor | undefined;
  measurements: Measurement[];
}

export const MeasurementGraph: React.FC<MeasurementGraphProps> = ({
  sensor,
  measurements,
}) => {
  const getMeasurements = () => {
    const temp = measurements
      .filter((x) => x.typeId === MeasurementTypes.Temperature)
      .map((x) => {
        // Get humidity, assume same datetime
        const humidityRow = measurements.find(
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

  const hasHumidity = measurements.some(
    (d) => d.typeId === MeasurementTypes.Humidity
  );

  const temperatures = measurements
    .filter((d) => d.typeId === MeasurementTypes.Temperature)
    .map((x) => x.sensorValue);

  const humidityValues = measurements
    .filter((d) => d.typeId === MeasurementTypes.Humidity)
    .map((x) => x.sensorValue);

  const minTemperature =
    temperatures.length > 0 ? Math.min(...temperatures) : undefined;
  const maxTemperature =
    temperatures.length > 0 ? Math.max(...temperatures) : undefined;

  const minHumidity =
    humidityValues.length > 0 ? Math.min(...humidityValues) : undefined;
  const maxHumidity =
    humidityValues.length > 0 ? Math.max(...humidityValues) : undefined;

  return (
    <Box
      display="flex"
      flexDirection="column"
      alignItems="center"
      flex={1}
      flexGrow={1}
      sx={{ height: "100%", width: "100%", maxHeight: "800px" }}
    >
      <Box
        flex={1}
        flexGrow={1}
        height={"100%"}
        width={"100%"}
        display={"flex"}
        flexDirection={"column"}
        maxHeight={"800px"}
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
      <Box width="100%" mt={2}>
        <Typography variant="h6" align="left" gutterBottom>
          Min/Max Values
        </Typography>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Measurement</TableCell>
                <TableCell align="center">Minimum</TableCell>
                <TableCell align="center">Maximum</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              <TableRow>
                <TableCell>Temperature (°C)</TableCell>
                <TableCell align="center">{minTemperature}</TableCell>
                <TableCell align="center">{maxTemperature}</TableCell>
              </TableRow>
              {hasHumidity && (
                <TableRow>
                  <TableCell>Humidity (%)</TableCell>
                  <TableCell align="center">{minHumidity}</TableCell>
                  <TableCell align="center">{maxHumidity}</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
    </Box>
  );
};
