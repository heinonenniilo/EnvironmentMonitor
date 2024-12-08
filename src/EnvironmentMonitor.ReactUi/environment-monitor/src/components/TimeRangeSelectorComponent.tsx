import { Box, Button } from "@mui/material";
import { TimeSelections } from "../enums/timeSelections";

export interface TimeRangeSelectorComponentProps {
  timeRange: TimeSelections;
  onSelectTimeRange: (selection: TimeSelections) => void;
}

export const TimeRangeSelectorComponent: React.FC<
  TimeRangeSelectorComponentProps
> = ({ timeRange, onSelectTimeRange }) => {
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "start",
        alignItems: "start",
        gap: 2,
      }}
    >
      <Button
        size="small"
        onClick={() => {
          onSelectTimeRange(TimeSelections.Hour24);
        }}
        variant={timeRange === TimeSelections.Hour24 ? "contained" : "outlined"}
      >
        24 h
      </Button>
      <Button
        size="small"
        variant={timeRange === TimeSelections.Hour48 ? "contained" : "outlined"}
        onClick={() => {
          onSelectTimeRange(TimeSelections.Hour48);
        }}
      >
        48 h
      </Button>
      <Button
        size="small"
        onClick={() => {
          onSelectTimeRange(TimeSelections.Hour72);
        }}
        variant={timeRange === TimeSelections.Hour72 ? "contained" : "outlined"}
      >
        72 h
      </Button>
    </Box>
  );
};
