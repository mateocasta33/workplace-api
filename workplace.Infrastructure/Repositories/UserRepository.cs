using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using workplace.Domain.Entities;
using workplace.Domain.Interfaces;

namespace workplace.Infrastructure.Repositories;

public class UserRepository: IUserRepository
{

    private readonly TableClient _tableClient;
    private readonly ILogger<UserRepository> _logger;
    private readonly string connectionString;

    public UserRepository(ILogger<UserRepository> logger)
    {
        _logger = logger;
        connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");

        _tableClient = new TableClient(connectionString, "Users");
    }
    
    public async Task<IEnumerable<User?>> GetAllUsersAsync()
    {
        var users = new List<User>();
        
        try
        {
            _logger.LogInformation("Obteniendo usuarios de la base de datos");
            await foreach (var user in _tableClient.QueryAsync<User>())
            {
                users.Add(user);
            }

            _logger.LogInformation("Usuarios obtenidos de forma exitosa");
            return users;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Error al optener los usuarios");
            throw new InvalidOperationException("Error al optener los usuarios", ex);
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _tableClient.GetEntityAsync<User>("USER", email);
            _logger.LogInformation("Usuario obtenido de forma exitosa");
            return user.Value;
        }
        catch (RequestFailedException ex) when(ex.Status == 404)
        {
            _logger.LogError("Usuario inexistente");
            return null;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Error al obtener al usuario");
            throw new InvalidOperationException("Error al obtener al usuario", ex);
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await _tableClient.GetEntityAsync<User>("USER", id.ToString());
            _logger.LogInformation("Usuario obtenido de forma exitosa");
            return user.Value;
        }
        catch (RequestFailedException ex) when(ex.Status == 404)
        {
            _logger.LogError("Usuario inexistente");
            throw new InvalidOperationException("Usuario inexistente");
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Error al obtener al usuario de la base de datos");
            throw new InvalidOperationException("Error al obtener al usuario de la base de datos");
        }
    }

    public async Task<User?> UpdateUserAsync(User user, ETag eTag)
    {
        try
        {
            await _tableClient.UpdateEntityAsync(user, eTag);
            _logger.LogInformation("Usuario actualizado exitosamente");
            return user;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogError("Intento de actualizar usuario iexistente");
            throw new InvalidOperationException("Usuario inexistente", ex);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Error al intentar actualizar el usuario");
            throw new InvalidOperationException("Error al intentar actualizar el usuario", ex);
        }
    }

    public async Task<bool> DeleteUserAsync(User user)
    {
        try
        {
            await _tableClient.DeleteEntityAsync(user.PartitionKey, user.RowKey);
            _logger.LogInformation("Usuario eliminado de forma exitosa");
            return true;
        }
        catch (RequestFailedException ex) when(ex.Status == 404)
        {
            _logger.LogError("Intento de eliminar usuario inexistente");
            throw new InvalidOperationException("Intento de eliminar usuario inexistente", ex);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Error al eliminar el usuario");
            throw new InvalidOperationException("Error al eliminar el usuario", ex);
        }
    }

    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            await _tableClient.AddEntityAsync(user);
            _logger.LogInformation("Usuario creado de forma exitosa");
            return user;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError("Error al crear usuario");
            throw new InvalidOperationException("Error al crear usuario", ex);
        }
    }
}