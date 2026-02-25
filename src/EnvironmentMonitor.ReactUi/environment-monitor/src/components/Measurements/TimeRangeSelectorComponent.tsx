import { Box, Button, Menu, MenuItem } from "@mui/material";
import { useState } from "react";

export interface TimeRangeSelectorComponentProps {
  timeRange: number;
  onSelectTimeRange: (selection: number) => void;
  options?: number[];
  labels?: string[];
  selectedText?: string;
}

const timeRangeOptions = [6, 12, 24, 48, 72];

export const TimeRangeSelectorComponent: React.FC<
  TimeRangeSelectorComponentProps
> = ({ timeRange, onSelectTimeRange, options, labels, selectedText }) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = (value?: number) => {
    setAnchorEl(null);
    if (value !== undefined) {
      onSelectTimeRange(value);
    }
  };

  return (
    <Box>
      <Button variant="outlined" size="small" onClick={handleClick}>
        {selectedText ?? `${timeRange} h`}
      </Button>
      <Menu anchorEl={anchorEl} open={open} onClose={() => handleClose()}>
        {(options ?? timeRangeOptions).map((option, idx) => (
          <MenuItem
            key={option}
            selected={timeRange === option}
            onClick={() => handleClose(option)}
          >
            {labels !== undefined && labels.length > idx
              ? labels[idx]
              : `${option} h`}
          </MenuItem>
        ))}
      </Menu>
    </Box>
  );
};
