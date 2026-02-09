namespace workplace.Infrastructure.Interfaces;

public interface IPlaceServiceInfrastructure
{
    Task<string> UploadPoster(Stream posterStream, string posterFileName);
    Task<string> UploadVideo(Stream posterStream, string posterFileName);
}