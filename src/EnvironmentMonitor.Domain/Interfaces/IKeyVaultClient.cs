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
        Task<StoreSecretReturnModel> StoreSecretAsync(string secretName, string secretValue);
        Task<StoreSecretReturnModel> StoreStreamAsSecretAsync(string secretName, Stream stream);
        Task<AttachmentDownloadModel> GetSecretAsStreamAsync(string secretName);
        Task<bool> DeleteSecretAsync(string secretName);
    }
}