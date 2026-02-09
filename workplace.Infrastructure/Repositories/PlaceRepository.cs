using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using workplace.Domain.Entities;
using workplace.Domain.Interfaces;

namespace workplace.Infrastructure.Repositories;

public class PlaceRepository : IPlaceRepository
{
    private readonly TableClient _tableClient;
    private readonly ILogger<PlaceRepository> _logger;
    private readonly IConfiguration _config;

    public PlaceRepository(ILogger<PlaceRepository> logger, IConfiguration config)
    {
        _logger = logger;
        var connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
        _tableClient = new TableClient(connectionString, "Places");
    }
    
    /// <summary>
    /// Obtener todos los espacios de la base de datos
    /// </summary>
    public async Task<IEnumerable<Place>> GetAllPlacesAsync()
    {
        try
        {
            var places = new List<Place>();

            _logger.LogInformation("Obteniendo los espacios de la base de datos");
            await foreach (var place in _tableClient.QueryAsync<Place>())
            {
                places.Add(place);
            }

            return places;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error al obtener los espacios de la base de datos");
            throw new InvalidOperationException("Error al obtener los espacios de la base de datos");
        }
    }

    /// <summary>
    /// Obtener espacio por id
    /// </summary>
    public async Task<Place> GetPlaceByIdAsync(string id)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<Place>("PLACE", id);
            return response.Value;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex,"Error al obtener el espacio de la base de datos");
            throw new InvalidOperationException("Error al obtener el espacio de la base de datos");
        }
    }

    public async Task<Place> CreatePlaceAsync(Place place)
    {
        try
        {
            var response = await _tableClient.AddEntityAsync(place);
            _logger.LogInformation("Espacio creado de forma exitosa");
            return place;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error al crear el espacio en la base de datos");
            throw new InvalidOperationException("Error al crear el espacio en la base de datos");
        }
    }

    public async Task<Place> UpdatePlaceAsync(Place place)
    {
        try
        {
            await _tableClient.UpdateEntityAsync(place, place.ETag);
            _logger.LogInformation("Espacio actualizado de forma exitosa");
            return place;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el espacio en la base de datos");
            throw new InvalidOperationException("Error al actualizar el espacio en la base de datos");
        }
    }

    public async Task<bool> DeletePlaceAsync(string movieId)
    {
        try
        {
            await _tableClient.DeleteEntityAsync("PLACE", movieId);
            _logger.LogInformation("Espacio eliminado de forma exitosa");
            return true;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error al eliminar el espacio de la base de datos");
            throw new InvalidOperationException("Error al eliminar el espacio de la base de datos");
        }
    }
}