using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class SendEmailOptions
    {
        public List<string> ToAddresses { get; set; } = new List<string>();
        public List<string> CcAddresses { get; set; } = new List<string>();
        public List<string> BccAddresses { get; set; } = new List<string>();
        public string Subject { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string PlainTextContent { get; set; } = string.Empty;
        public Dictionary<string, string>? ReplaceTokens { get; set; }
    }
}
