import { Chip, Stack } from "@mui/material";
import type { MfaMethod } from "../../models/user";
import { mfaMethodLabel } from "../../utilities/mfaUtils";

interface MfaMethodsDisplayProps {
  methods?: MfaMethod[];
}

export const MfaMethodsDisplay = ({ methods = [] }: MfaMethodsDisplayProps) => (
  <Stack direction="row" spacing={0.75} useFlexGap flexWrap="wrap" sx={{ py: 0.5 }}>
    {methods.length > 0 ? methods.map((method) => (
      <Chip key={method} label={mfaMethodLabel(method)} size="small" color="success" variant="outlined" />
    )) : (
      <Chip label="Disabled" size="small" variant="outlined" />
    )}
  </Stack>
);
