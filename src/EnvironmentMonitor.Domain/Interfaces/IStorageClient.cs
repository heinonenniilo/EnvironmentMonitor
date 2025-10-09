using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.AddModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IStorageClient
    {
        public Task<Uri> Upload(UploadAttachmentModel model);
        Task<AttachmentDownloadModel> GetImageAsync(string fileName);
        public Task<AttachmentInfoModel> GetBlobInfo(string fileName);
        Task<bool> DeleteBlob(string fileName);
    }
}
