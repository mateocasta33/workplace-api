using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using workplace.Infrastructure.Interfaces;

namespace workplace.Infrastructure.Services;

public class PlaceServiceInfrastructure : IPlaceServiceInfrastructure
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<PlaceServiceInfrastructure> _logger;
    private readonly string posterContainer ="places-posters";
    private readonly string videoContainer ="places-videos";
    
    public PlaceServiceInfrastructure(ILogger<PlaceServiceInfrastructure> logger)
    {
        _logger = logger;
        var connectionString = Environment.GetEnvironmentVariable("");

        _blobServiceClient = new BlobServiceClient(connectionString);
    }
    
    public async Task<string> UploadPoster(Stream posterStream, string posterFileName)
    {
        return await UploadBlobAsync(posterContainer, posterStream, posterFileName, "video/mp4");
    }

    public async Task<string> UploadVideo(Stream videoStream, string videoFileName)
    {
        return await UploadBlobAsync(videoContainer, videoStream, videoFileName, "video/mp4");
    }

    private async Task<string> UploadBlobAsync(string containerName, Stream fileStream, string fileName,
        string contentType)
    {
        try
        {
            var blobContainer =  _blobServiceClient.GetBlobContainerClient(containerName);

            var uniqueName = $"{Guid.NewGuid()}_{fileName}";
            var blobClient = blobContainer.GetBlobClient(uniqueName);

            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = httpHeaders
            });

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw new InvalidOperationException("Error al subir el archivo");
        }
    }
}