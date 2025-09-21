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

export const PublicMeasurementsView: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const apiHook = useApiHook().measureHook;
  const dispatch = useDispatch();
  const timeRange = useSelector(getDashboardTimeRange);
  const [model, setModel] = useState<MeasurementsViewModel | undefined>(
    undefined
  );
  const handleTimeRangeChange = (selection: number) => {
    dispatch(setDashboardTimeRange(selection));
  };

  useEffect(() => {
    if (apiHook) {
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
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeRange]);

  return (
    <AppContentWrapper
      title="Latest measurements"
      isLoading={isLoading}
      titleComponent={
        <TimeRangeSelectorComponent
          timeRange={timeRange}
          onSelectTimeRange={handleTimeRangeChange}
        />
      }
    >
      <MultiSensorGraph
        sensors={model?.sensors ?? []}
        model={model}
        title=" "
        useAutoScale
        hideUseAutoScale
      />
    </AppContentWrapper>
  );
};
