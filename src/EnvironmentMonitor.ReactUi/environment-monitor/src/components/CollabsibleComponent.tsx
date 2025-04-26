import { useState } from "react";
import { Box, Typography, IconButton, Collapse } from "@mui/material";
import { ExpandMore } from "@mui/icons-material";
import { styled } from "@mui/material/styles";

export interface CollapsibleProps {
  title: string;
  children: React.ReactNode;
  isOpen?: boolean;
  customComponent?: JSX.Element;
}

const ExpandMoreIconButton = styled((props: any) => {
  const { expand, ...other } = props;
  return <IconButton {...other} />;
})(({ theme, expand }: { theme?: any; expand: boolean }) => ({
  transform: expand ? "rotate(180deg)" : "rotate(0deg)",
  transition: theme.transitions.create("transform", {
    duration: theme.transitions.duration.shortest,
  }),
  marginRight: theme.spacing(1),
}));

export const Collapsible: React.FC<CollapsibleProps> = ({
  title,
  children,
  isOpen,
  customComponent,
}) => {
  const [open, setOpen] = useState(isOpen ?? false);

  const toggleOpen = () => {
    setOpen((prev) => !prev);
  };

  return (
    <Box sx={{ mb: 2, mt: 2 }}>
      <Box display="flex" alignItems="center" sx={{ mb: 2 }}>
        <Box sx={{ display: "flex", flexDirection: "row" }}>
          <ExpandMoreIconButton
            expand={open ? 1 : 0}
            size="small"
            sx
            onClick={() => {
              toggleOpen();
            }}
          >
            <ExpandMore />
          </ExpandMoreIconButton>
          <Box sx={{ display: "flex", flexDirection: "row" }}>
            <Typography variant="h6">{title}</Typography>
          </Box>
        </Box>
        {customComponent}
      </Box>

      <Collapse in={open}>
        <Box>{children}</Box>
      </Collapse>
    </Box>
  );
};
