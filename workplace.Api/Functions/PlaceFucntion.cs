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
                await response.WriteAsJsonAsync(new {error = "Invalid request context"});
                return response;
            }

            var form = await httpContext.Request.ReadFormAsync();

            if (!form.ContainsKey("name") || !form.ContainsKey("description") ||
                !form.ContainsKey("capacity") || !form.ContainsKey("isActive"))
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Faltan datos requeridos"});
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
                name = form.Files["name"].ToString(),
                description = form.Files["description"].ToString(),
                capacity = Convert.ToInt32(form.Files["capacity"]),
                isActive = Convert.ToBoolean(form.Files["isActive"]),
            };

            using var posterStream = posterFile.OpenReadStream();
            using var videoStream = videoFile.OpenReadStream();

            var place = await _placeService.CreatePlaceAsync(placeCreateDto, posterStream, videoStream);

            response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(place);
            return response;
        }
    }
}