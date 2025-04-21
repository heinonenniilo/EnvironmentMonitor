using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IStorageClient
    {
        public Task<Uri> Upload(Stream stream, string blobName);
        Task<AttachmentInfoModel> GetImageAsync(string fileName);
    }
}
