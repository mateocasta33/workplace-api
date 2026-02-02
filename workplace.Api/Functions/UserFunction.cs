using System.Net;
using System.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using workplace.Application.Dtos;
using workplace.Application.Interfaces;

namespace workplace_Api.Functions;

public class UserFunction
{
    private readonly ILogger<UserFunction> _logger;
    private readonly IUserService _userService;

    public UserFunction(
        ILogger<UserFunction> logger,
        IUserService userService
        )
    {
        _logger = logger;
        _userService = userService;
    }

    [Function("GetAllUsers")]
    public async Task<HttpResponseData> GetAllUsers(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "users")]
        HttpRequestData req
        )
    {
        try
        {
            _logger.LogInformation("Obteniendo todos los usuarios");
            var users = await _userService.GetAllUsers();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(users);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error al obtener los usuarios");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor"});
            return response;
        }
    }

    [Function("GetUserById")]
    public async Task<HttpResponseData> GetUserById(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "users/{id}")]
        HttpRequestData req,
        string id
        )
    {
        try
        {
            _logger.LogInformation($"Obteniendo el usuario con id: {id}");
            var user = await _userService.GetUserById(id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(user);
            return response;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError($"Usuario con id: {id} no encontrado");
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error interno del servidor");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor"});
            return response;
        }
    }

    [Function("RegisterUser")]
    public async Task<HttpResponseData> RegisterUser(
        [HttpTrigger(
            AuthorizationLevel.Anonymous, 
            "post",
            Route = "users/register")]
        HttpRequestData req
        )
    {
        try
        {
            var user = await req.ReadFromJsonAsync<UserRegisterRequestDto>();

            if (user == null)
            {
                _logger.LogError("El cuerpo de la peticion es incorrecto");
                var responseError = req.CreateResponse(HttpStatusCode.BadRequest);
                await responseError.WriteAsJsonAsync(new { error = "El cuerpo de la peticion es incorrecto" });
                return responseError;
            }
            
            _logger.LogInformation("Registrando usuario");
            
            var userRegister = await _userService.CreateUser(user);
            
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(userRegister);
            return response;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = ex.Message});
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = ex.Message});
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error interno del servidor", ex);
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor" });
            return response;
        }
    }

    [Function("LoginUser")]
    public async Task<HttpResponseData> LoginUser(
        [HttpTrigger(AuthorizationLevel.Anonymous,
            "post",
            Route = "users/login")]
        HttpRequestData req
        )
    {
        try
        {

            var request = await req.ReadFromJsonAsync<UserLoginRequestDto>();
            
            if (request == null)
            {
                _logger.LogError("El cuerpo de la peticion es incorrecto");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new { error = "El cuerpo de la peticion es incorrecto" });
                return errorResponse;
            }
            
            _logger.LogInformation("Usuario logeandose");
            
            var userLogin = await _userService.LoginUser(request);
            
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(userLogin);
            return response;
        }
        catch (SecurityException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor" });
            return response;
        }
    }

    [Function("UpdateUser")]
    public async Task<HttpResponseData> UpdateUser(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "put",
            Route = "users/update"
            )]
        HttpRequestData req
        )
    {
        try
        {

            var request = await req.ReadFromJsonAsync<UserRegisterRequestDto>();
            
            if (request == null)
            {
                _logger.LogError("El cuerpo de la peticion es incorrecto");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new { error = "El cuerpo de la peticion es incorrecto" });
                return errorResponse;
            }
            
            var userUpdate = await _userService.UpdateUser(request);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(userUpdate);
            return response;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error interno del servidor", ex.Message);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor"});
            return response;
        }
    }

    [Function("DeleteUser")]
    public async Task<HttpResponseData> DeleteUser(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "delete",
            Route = "users/{email}")]
        HttpRequestData req,
        string email)
    {
        try
        {
            var isDeleted = await _userService.DeleteUser(email);
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            await response.WriteAsJsonAsync(new { deleted = false});
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor"});
            return response;
        }
    }
}