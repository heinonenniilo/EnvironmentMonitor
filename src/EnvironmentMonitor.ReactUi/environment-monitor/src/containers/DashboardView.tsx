import { useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect, useState } from "react";
import { getSensors } from "../reducers/measurementReducer";
import { useApiHook } from "../hooks/apiHook";
import { Box } from "@mui/material";
import { MeasurementGraph } from "../components/MeasurementGraph";
import moment from "moment";
import { MeasurementsViewModel } from "../models/measurementsBySensor";

export const DashboardView: React.FC = () => {
  const measurementApiHook = useApiHook().measureHook;
  const [viewModel, setViewModel] = useState<MeasurementsViewModel | undefined>(
    undefined
  );

  const sensors = useSelector(getSensors);

  useEffect(() => {
    if (sensors.length > 0) {
      console.log("Fetching");

      measurementApiHook
        .getMeasurementsBySensor(
          sensors.map((x) => x.id),
          moment().utc().startOf("day").toDate(),
          moment().utc().endOf("day").toDate()
        )
        .then((res) => {
          setViewModel(res);
        })
        .catch((er) => {
          console.error(er);
          setViewModel(undefined);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [sensors]);

  return (
    <AppContentWrapper titleParts={[{ text: "Home" }]}>
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: {
            xs: "1fr", // Single column for extra-small screens
            // sm: "1fr 1fr", // Two columns for small and larger screens
            md: "1fr 1fr",
          },
          gap: 2, // Space between grid items
          padding: 2, // Padding around the grid container
        }}
      >
        {sensors.map((sensor) => (
          <Box
            key={sensor.id}
            sx={{
              border: "1px solid #ccc",
              borderRadius: 2,
              padding: 2,
              boxSizing: "border-box",
              backgroundColor: "#f9f9f9",
              // height: "800px", // Fixed height for graphs
              height: "100%",
              display: "flex",
              flexDirection: "column",
              justifyContent: "center",
              alignItems: "center",
            }}
          >
            <MeasurementGraph
              sensor={sensor}
              measurements={
                viewModel
                  ? viewModel.measurements?.find(
                      (d) => d.sensorId === sensor.id
                    )?.measurements ?? []
                  : []
              }
            />
          </Box>
        ))}
      </Box>
    </AppContentWrapper>
  );
};
