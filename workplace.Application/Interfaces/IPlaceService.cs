using workplace.Application.Dtos;

namespace workplace.Application.Interfaces;

public interface IPlaceService
{
    Task<PlaceResponseDto?> CreatePlaceAsync(PlaceCreateDto placeCreateDto, Stream posterStream, Stream videoStream);
    Task<IEnumerable<PlaceResponseDto?>> GetAllPlacesAsync();
    Task<PlaceResponseDto?> GetPlaceByIdAsync(string placeId);
    Task<bool> DeletePlaceAsync(string placeId);
}