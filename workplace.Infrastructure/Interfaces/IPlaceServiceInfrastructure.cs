namespace workplace.Infrastructure.Interfaces;

public interface IPlaceServiceInfrastructure
{
    Task<string> UploadPosterAsync(Stream fileStream, string fileName);
    Task<string> UploadVideoAsync(Stream fileStream, string fileName);
}