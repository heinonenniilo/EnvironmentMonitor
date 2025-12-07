import { useEffect, useState } from "react";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useApiHook } from "../hooks/apiHook";
import type { MeasurementsViewModel } from "../models/measurementsBySensor";
import {
  getDashboardTimeRange,
  setDashboardTimeRange,
} from "../reducers/measurementReducer";
import { useDispatch, useSelector } from "react-redux";
import { TimeRangeSelectorComponent } from "../components/TimeRangeSelectorComponent";
import moment from "moment";
import { MultiSensorGraph } from "../components/MultiSensorGraph";
import { Box, IconButton, Tooltip } from "@mui/material";
import { Fullscreen } from "@mui/icons-material";
export const PublicMeasurementsView: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [isFullScreen, setIsFullScreen] = useState(false);
  const apiHook = useApiHook().measureHook;
  const dispatch = useDispatch();
  const timeRange = useSelector(getDashboardTimeRange);
  const [model, setModel] = useState<MeasurementsViewModel | undefined>(
    undefined
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
      titleComponent={
        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
          <TimeRangeSelectorComponent
            timeRange={timeRange}
            onSelectTimeRange={handleTimeRangeChange}
          />
          <Tooltip title="Full screen">
            <IconButton onClick={() => setIsFullScreen(true)} size="medium">
              <Fullscreen />
            </IconButton>
          </Tooltip>
        </Box>
      }
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
      />
    </AppContentWrapper>
  );
};
