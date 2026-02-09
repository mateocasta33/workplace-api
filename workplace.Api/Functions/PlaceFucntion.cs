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
            var httpContext = context.GetHttpContext();
            
            var form = await httpContext.Request.ReadFormAsync();

            var fileVideo = form.Files["video"];
            var filePoster = form.Files["poster"];

            if (filePoster == null || fileVideo == null)
            {
                _logger.LogError("Todos los campos son requeridos");
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "El poster y el video son requeridos" });
                return response;
            }

            if (!form.ContainsKey("name") || !form.ContainsKey("description") || !form.ContainsKey("capacity") ||
                !form.ContainsKey("isActive") || !form.ContainsKey("posterFileName") ||
                !form.ContainsKey("videoFileName"))
            {
                _logger.LogError("Todos los campos son requeridos");
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Todos los cambos son requeridos"});
                return response;
            }

            if (!form.ContainsKey("isActive") ||
                !bool.TryParse(form["isActive"], out var isActive))
            {
                _logger.LogError("Todos los campos son requeridos");
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Todos los cambos son requeridos"});
                return response;
            }

            if (!form.ContainsKey("capacity") ||
                !int.TryParse(form["capacity"], out var capacity))
            {
                _logger.LogError("Todos los campos son requeridos");
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Todos los cambos son requeridos"});
                return response;
            }

            var newPlace = new PlaceCreateDto
            {
                name = form["name"].ToString(),
                capacity = capacity,
                description = form["description"].ToString(),
                posterFileName = form["posterFileName"].ToString(),
                videoFileName = form["videoFileName"].ToString(),
                isActive = isActive,
            };

            var posterStream = filePoster.OpenReadStream();
            var videoStream = fileVideo.OpenReadStream();
            
            _logger.LogInformation("Espacio creandoce");
            var result = await _placeService.CreatePlaceAsync(newPlace, posterStream, videoStream);

            response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(result);
            return response;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
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