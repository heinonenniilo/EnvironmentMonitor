using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IImageService
    {
        public Task<Stream> CompressToSize(Stream imageStream, uint sizeInMbs);

        public Task<Stream> CompressToSize(Stream imageStream);
    }
}
