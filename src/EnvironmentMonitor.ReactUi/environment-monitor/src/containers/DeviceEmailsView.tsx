import { AppContentWrapper } from "../framework/AppContentWrapper";
import { useEffect, useState } from "react";
import { useApiHook } from "../hooks/apiHook";
import { type DeviceEmailTemplateDto } from "../models/deviceEmailTemplate";
import { DeviceEmailTemplatesTable } from "../components/DeviceEmails/DeviceEmailTemplatesTable";
import { EditEmailTemplateDialog } from "../components/DeviceEmails/EditEmailTemplateDialog";
import { useDispatch } from "react-redux";
import { addNotification } from "../reducers/userInterfaceReducer";

export const DeviceEmailsView: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [templates, setTemplates] = useState<DeviceEmailTemplateDto[]>([]);
  const [selectedTemplate, setSelectedTemplate] =
    useState<DeviceEmailTemplateDto | null>(null);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const deviceEmailsHook = useApiHook().deviceEmailsHook;
  const dispatch = useDispatch();

  useEffect(() => {
    getEmailTemplates();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getEmailTemplates = () => {
    setIsLoading(true);
    deviceEmailsHook
      .getAllEmailTemplates()
      .then((res) => {
        setTemplates(res);
      })
      .catch((er) => {
        console.error(er);
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleEdit = (template: DeviceEmailTemplateDto) => {
    setSelectedTemplate(template);
    setIsDialogOpen(true);
  };

  const handleSave = (identifier: string, title: string, message: string) => {
    setIsLoading(true);
    deviceEmailsHook
      .updateEmailTemplate({
        identifier,
        title,
        message,
      })
      .then(() => {
        dispatch(
          addNotification({
            title: "Email template updated successfully",
            body: "",
            severity: "success",
          })
        );
        getEmailTemplates();
      })
      .catch((er) => {
        console.error(er);
        dispatch(
          addNotification({
            title: "Failed to update email template",
            body: "",
            severity: "error",
          })
        );
      })
      .finally(() => {
        setIsLoading(false);
      });
  };

  const handleCloseDialog = () => {
    setIsDialogOpen(false);
    setSelectedTemplate(null);
  };

  return (
    <AppContentWrapper title="Email Templates" isLoading={isLoading}>
      <DeviceEmailTemplatesTable templates={templates} onEdit={handleEdit} />
      <EditEmailTemplateDialog
        open={isDialogOpen}
        template={selectedTemplate}
        onClose={handleCloseDialog}
        onSave={handleSave}
      />
    </AppContentWrapper>
  );
};
