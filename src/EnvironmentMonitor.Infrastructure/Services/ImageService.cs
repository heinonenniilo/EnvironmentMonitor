using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using ImageMagick;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class ImageService : IImageService
    {
        private readonly FileUploadSettings _settings;
        private readonly ILogger<ImageService> _logger;

        public ImageService(FileUploadSettings settings, ILogger<ImageService> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public Task<Stream> CompressToSize(Stream inputStream, uint sizeInMbs)
        {
            return Task.Run(() =>
            {
                var maxSizeBytes = sizeInMbs * 1024 * 1024;
                _logger.LogInformation($"Compressing image. Max size in bytes: {maxSizeBytes}");
                var minWidth = _settings.ImageMinWidht;
                var minHeight = _settings.ImageMinHeight;

                using var imageToReturn = new MagickImage(inputStream);
                imageToReturn.Format = MagickFormat.Jpeg;
                imageToReturn.Quality = 100;

                var width = imageToReturn.Width;
                var height = imageToReturn.Height;

                double scaleFactor = 0.9;
                MemoryStream outputStream;

                while (true)
                {
                    outputStream = new MemoryStream();
                    imageToReturn.Write(outputStream);

                    if (outputStream.Length <= maxSizeBytes)
                        break;

                    width = (uint)(width * scaleFactor);
                    height = (uint)(height * scaleFactor);

                    if (width < minWidth || height < minHeight)
                        break;

                    var geometry = new MagickGeometry(width, height)
                    {
                        IgnoreAspectRatio = false
                    };

                    imageToReturn.Resize(geometry);
                    outputStream.Dispose();
                }

                outputStream.Position = 0;
                return (Stream)outputStream;
            });
        }

        public Task<Stream> CompressToSize(Stream inputStream)
        {
            return CompressToSize(inputStream, _settings.MaxImageSaveSizeMb);
        }
    }
}
