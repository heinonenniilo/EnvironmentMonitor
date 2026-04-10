import { useEffect, useState } from "react";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useApiHook } from "../hooks/apiHook";
import type { MeasurementsViewModel } from "../models/measurementsBySensor";
import {
  getDashboardTimeRange,
  getSelectedMeasurementTypes,
  setDashboardTimeRange,
} from "../reducers/measurementReducer";
import { useDispatch, useSelector } from "react-redux";
import { TimeRangeSelectorComponent } from "../components/Measurements/TimeRangeSelectorComponent";
import moment from "moment";
import { MultiSensorGraph } from "../components/Measurements/MultiSensorGraph";
import { MeasurementsMap } from "../components/Measurements/MeasurementsMap";
import { Box, IconButton, Tooltip } from "@mui/material";
import { Fullscreen, Refresh } from "@mui/icons-material";
export const PublicMeasurementsView: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [isFullScreen, setIsFullScreen] = useState(false);
  const [hoveredSensorIdentifier, setHoveredSensorIdentifier] = useState<
    string | null
  >(null);
  const apiHook = useApiHook().measureHook;
  const dispatch = useDispatch();
  const timeRange = useSelector(getDashboardTimeRange);
  const selectedMeasurementTypes = useSelector(getSelectedMeasurementTypes);
  const [model, setModel] = useState<MeasurementsViewModel | undefined>(
    undefined,
  );
  const handleTimeRangeChange = (selection: number) => {
    dispatch(setDashboardTimeRange(selection));
  };

  const loadMeasurements = () => {
    if (!apiHook) {
      return;
    }
    setIsLoading(true);
    const momentStart = moment()
      .local(true)
      .add(-1 * timeRange, "hour")
      .utc(true);
    apiHook
      .getPublicMeasurements(momentStart)
      .then((res) => {
        setModel(res);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };
  useEffect(() => {
    loadMeasurements();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeRange]);

  return (
    <AppContentWrapper
      title="Latest measurements"
      isLoading={isLoading}
      allowFullWidth
      titleComponent={
        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
          <TimeRangeSelectorComponent
            timeRange={timeRange}
            onSelectTimeRange={handleTimeRangeChange}
          />
          <Tooltip title="Refresh">
            <IconButton onClick={loadMeasurements} size="medium">
              <Refresh />
            </IconButton>
          </Tooltip>
          <Tooltip title="Full screen">
            <IconButton onClick={() => setIsFullScreen(true)} size="medium">
              <Fullscreen />
            </IconButton>
          </Tooltip>
        </Box>
      }
    >
      <Box
        sx={{
          display: "flex",
          flexDirection: { xs: "column", xl: "row" },
          gap: 2,
          width: "100%",
          flex: 1,
          height: "100%",
          alignItems: "stretch",
          maxHeight: "100%",
          paddingBottom: 2,
        }}
      >
        <Box
          sx={{
            flex: { xs: "1 1 auto", lg: "1 1 58%" },
            minWidth: 0,
            display: "flex",
            minHeight: 0,
          }}
        >
          <MultiSensorGraph
            sensors={model?.sensors ?? []}
            model={model}
            title={isFullScreen ? `Range: ${timeRange} hours` : " "}
            useAutoScale
            hideUseAutoScale
            minHeight={500}
            enableHighlightOnRowHover
            isFullScreen={isFullScreen}
            onSetFullScreen={(state) => setIsFullScreen(state)}
            showFullScreenIcon={false}
            highlightedSensorIdentifier={hoveredSensorIdentifier}
          />
        </Box>
        <Box
          sx={{
            flex: { xs: "1 1 auto", lg: "1 1 42%" },
            minWidth: 0,
            display: "flex",
            minHeight: 0,
          }}
        >
          <MeasurementsMap
            model={model}
            measurementTypes={selectedMeasurementTypes}
            minHeight={500}
            onHoveredSensorIdentifierChange={setHoveredSensorIdentifier}
          />
        </Box>
      </Box>
    </AppContentWrapper>
  );
};
