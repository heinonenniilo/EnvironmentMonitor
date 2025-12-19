using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IEmailClient
    {
        Task SendEmailAsync(List<string> emails, string subject, string htmlContent, string plainTextContent = "");
    }
}
