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
    _logger.LogInformation("=== INICIO CreatePlace ===");
    
    try
    {
        _logger.LogInformation("Paso 1: Obteniendo HttpContext");
        var httpContext = context.GetHttpContext();
        
        _logger.LogInformation("Paso 2: Leyendo formulario");
        var form = await httpContext.Request.ReadFormAsync();
        
        _logger.LogInformation("Paso 3: Obteniendo archivos");
        var fileVideo = form.Files["video"];
        var filePoster = form.Files["poster"];

        _logger.LogInformation($"Paso 4: Video={fileVideo?.FileName ?? "NULL"}, Poster={filePoster?.FileName ?? "NULL"}");

        if (filePoster == null || fileVideo == null)
        {
            _logger.LogError("Archivos faltantes");
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = "El poster y el video son requeridos" });
            return response;
        }

        _logger.LogInformation("Paso 5: Validando campos obligatorios");
        if (!form.ContainsKey("name") || !form.ContainsKey("description") || 
            !form.ContainsKey("capacity") || !form.ContainsKey("isActive") || 
            !form.ContainsKey("posterFileName") || !form.ContainsKey("videoFileName"))
        {
            _logger.LogError("Campos faltantes en el formulario");
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = "Todos los campos son requeridos"});
            return response;
        }

        _logger.LogInformation("Paso 6: Parseando capacity");
        if (!int.TryParse(form["capacity"], out var capacity))
        {
            _logger.LogError("Capacity inválido");
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = "Capacity inválido"});
            return response;
        }

        _logger.LogInformation("Paso 7: Parseando isActive");
        if (!bool.TryParse(form["isActive"], out var isActive))
        {
            _logger.LogError("isActive inválido");
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = "isActive inválido"});
            return response;
        }

        _logger.LogInformation("Paso 8: Creando DTO");
        var newPlace = new PlaceCreateDto
        {
            name = form["name"].ToString(),
            capacity = capacity,
            description = form["description"].ToString(),
            posterFileName = form["posterFileName"].ToString(),
            videoFileName = form["videoFileName"].ToString(),
            isActive = isActive,
        };

        _logger.LogInformation("Paso 9: Abriendo streams");
        var posterStream = filePoster.OpenReadStream();
        var videoStream = fileVideo.OpenReadStream();
        
        _logger.LogInformation("Paso 10: ANTES de llamar a CreatePlaceAsync");
        var result = await _placeService.CreatePlaceAsync(newPlace, posterStream, videoStream);
        _logger.LogInformation("Paso 11: DESPUÉS de llamar a CreatePlaceAsync");

        var successResponse = req.CreateResponse(HttpStatusCode.Created);
        await successResponse.WriteAsJsonAsync(result);
        
        _logger.LogInformation("=== FIN CreatePlace EXITOSO ===");
        return successResponse;
    }
    catch (Exception e)
    {
        _logger.LogError($"❌ EXCEPCIÓN CAPTURADA: {e.GetType().Name}");
        _logger.LogError($"❌ MENSAJE: {e.Message}");
        _logger.LogError($"❌ STACK TRACE: {e.StackTrace}");
        
        if (e.InnerException != null)
        {
            _logger.LogError($"❌ INNER EXCEPTION: {e.InnerException.Message}");
        }
        
        var response = req.CreateResponse(HttpStatusCode.InternalServerError);
        await response.WriteAsJsonAsync(new { error = e.Message });
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
            _logger.LogError(ex.Message);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Error interno del servidor" });
            return response;
        }
    }
    
}