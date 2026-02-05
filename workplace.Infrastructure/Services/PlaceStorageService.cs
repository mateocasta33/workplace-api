using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using workplace.Infrastructure.Interfaces;

namespace workplace.Infrastructure.Services;

public class PlaceStorageService : IPlaceServiceInfrastructure
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<PlaceStorageService> _logger;
    private readonly string PostersContainer = "places-posters";
    private readonly string VideoContainer = "places-videos";

    public PlaceStorageService(ILogger<PlaceStorageService> logger)
    {
        _logger = logger;
        var connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");

        _blobServiceClient = new BlobServiceClient(connectionString);
    }
    
        
    public async Task<string> UploadPosterAsync(Stream fileStream, string fileName)
    {
        return await UploadBlobAsync(PostersContainer, fileStream, fileName, "image/jpeg");
    }

    public async Task<string> UploadVideoAsync(Stream fileStream, string fileName)
    {
        return await UploadBlobAsync(VideoContainer, fileStream, fileName, "video/mp4");
    }

    private async Task<string> UploadBlobAsync(string containerName, Stream filestream, string filename, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var uniqueFileName = $"{Guid.NewGuid()}_{filename}";
        var blobClient = containerClient.GetBlobClient(uniqueFileName);

        var blobHttpHeader = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        await blobClient.UploadAsync(filestream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeader
        });

        return blobClient.Uri.ToString();
    }
}