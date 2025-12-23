import { Box, Paper, Typography, Divider } from "@mui/material";

export interface EmailTemplatePreviewProps {
  title: string;
  message: string;
}

export const EmailTemplatePreview: React.FC<EmailTemplatePreviewProps> = ({
  title,
  message,
}) => {
  return (
    <Box flex={1} display="flex" flexDirection="column" gap={2} minWidth="50%">
      <Typography variant="h6" color="text.secondary">
        Preview
      </Typography>
      <Paper
        elevation={0}
        sx={{
          flex: 1,
          p: 2,
          bgcolor: "background.default",
          border: "1px solid",
          borderColor: "divider",
          overflow: "auto",
        }}
      >
        <Typography variant="h5" gutterBottom>
          {title || "(No title)"}
        </Typography>
        <Divider sx={{ my: 2 }} />
        <Box
          dangerouslySetInnerHTML={{
            __html: message || "<em>No message content</em>",
          }}
          sx={{
            "& *": {
              maxWidth: "100%",
            },
          }}
        />
      </Paper>
    </Box>
  );
};
