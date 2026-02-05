using System.Net;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using workplace.Application.Dtos;
using workplace.Application.Interfaces;

namespace workplace_Api.Functions;

public class PlaceFucntion
{
    private readonly ILogger<PlaceFucntion> _logger;
    private readonly IPlaceService _placeService;

    public PlaceFucntion(ILogger<PlaceFucntion> logger, IPlaceService placeService)
    {
        _logger = logger;
        _placeService = placeService;
    }

    [Function("CreatePlace")]
    public async Task<HttpResponseData> CreatePlace(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "places")]
        HttpRequestData req,
        FunctionContext context)
    {
        try
        {

            HttpResponseData response;
            // Obtenemos el HttpContext para poder acceder al form-data
            var httpContext = context.GetHttpContext();

            if (httpContext == null)
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Invalid request context" });
                return response;
            }

            var form = await httpContext.Request.ReadFormAsync();

            if (!form.ContainsKey("name") || !form.ContainsKey("description") ||
                !form.ContainsKey("capacity") || !form.ContainsKey("isActive"))
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Faltan datos requeridos" });
                return response;
            }

            var posterFile = form.Files["poster"];
            var videoFile = form.Files["video"];

            if (posterFile == null || videoFile == null)
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Poster y video son requeridos" });
                return response;
            }

            var placeCreateDto = new PlaceCreateDto
            {
                name = form["name"].ToString(),  
                description = form["description"].ToString(),
                capacity = Convert.ToInt32(form["capacity"]),
                isActive = Convert.ToBoolean(form["isActive"]),
            };

            using var posterStream = posterFile.OpenReadStream();
            using var videoStream = videoFile.OpenReadStream();

            var place = await _placeService.CreatePlaceAsync(placeCreateDto, posterStream, videoStream);

            response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(place);
            return response;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno del servidor");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor" });
            return response;
        }
        
    }

    [Function("GetAllPlaces")]
    public async Task<HttpResponseData> GetAllPlaces(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "places")]
        HttpRequestData req )
    {
        try
        {
            HttpResponseData responseData;
            
            var places = await _placeService.GetAllPlacesAsync();
            
            _logger.LogInformation("Espacios obtenidos de forma exitosa");
            
            responseData = req.CreateResponse(HttpStatusCode.OK);
            await responseData.WriteAsJsonAsync(places);
            return responseData;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los espacios");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor " });
            return response;
        }
    }

    [Function("GetPlaceById")]
    public async Task<HttpResponseData> GetUserById(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "places/{id}")]
        HttpRequestData req,
        string id)
    {
        HttpResponseData responseData;

        try
        {
            var place = await _placeService.GetPlaceByIdAsync(id);

            _logger.LogInformation("Espacio obtenido de forma exitosa");

            responseData = req.CreateResponse(HttpStatusCode.Accepted);
            await responseData.WriteAsJsonAsync(place);
            return responseData;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError("Usuario no encontrado");
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno del servidor");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor" });
            return response;
        }
    }

    [Function("DeletePlace")]
    public async Task<HttpResponseData> DeletePlace(
        [HttpTrigger(AuthorizationLevel.Anonymous,
            "delete",
            Route = "places/{id}")]
        HttpRequestData req,
        string id
        )
    {
        try
        {
            HttpResponseData response;

            var IsDeleted = await _placeService.DeletePlaceAsync(id);

            _logger.LogInformation("Usuario eliminado de forma exitosa");

            response = req.CreateResponse(HttpStatusCode.NoContent);
            await response.WriteAsJsonAsync(new { isDeleted = true });
            return response;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno del servidor");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor" });
            return response;
        }
    }
    
}