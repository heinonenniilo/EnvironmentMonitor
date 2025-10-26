using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IKeyVaultClient
    {
        Task<string> GetSecretAsync(string secretName);
        Task<string> StoreSecretAsync(string secretName, string secretValue);
        Task<string> StoreStreamAsSecretAsync(string secretName, Stream stream);
        Task<AttachmentDownloadModel> GetSecretAsStreamAsync(string secretName);
        Task<string> StoreTextFileAsSecretAsync(string secretName, Stream stream, Encoding? encoding = null);
    }
}