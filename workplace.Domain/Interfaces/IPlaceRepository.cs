using Azure;
using workplace.Domain.Entities;

namespace workplace.Domain.Interfaces;

public interface IPlaceRepository
{
    Task<IEnumerable<Place?>> GetAllPlacesAsync();
    Task<Place?> GetPlaceByIdAsync(string id);
    Task<Place> CreatePlaceAsync(Place place);
    Task<Place?> UpdatePlaceAsync(Place place);
    Task<bool> DeletePlaceAsync(string movieId);
}