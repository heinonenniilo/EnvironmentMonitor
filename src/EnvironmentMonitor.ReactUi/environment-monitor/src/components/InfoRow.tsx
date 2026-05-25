import { Box, Checkbox, Typography } from "@mui/material";
import { Link } from "react-router";

interface InfoRowProps {
  label: string;
  value?: string | number;
  to?: string;
  checked?: boolean;
}

const labelWidth = 72;

export const InfoRow: React.FC<InfoRowProps> = ({
  label,
  value,
  checked,
  to,
}) => (
  <Box display="flex" alignItems="center" gap={2}>
    <Typography
      variant="body2"
      sx={{ minWidth: labelWidth, fontWeight: 600, color: "text.secondary" }}
    >
      {label}
    </Typography>
    {checked !== undefined ? (
      <Checkbox checked={checked} disabled size="small" sx={{ p: 0 }} />
    ) : (
      <Typography variant="body2">
        {to ? <Link to={to}>{value}</Link> : value}
      </Typography>
    )}
  </Box>
);
