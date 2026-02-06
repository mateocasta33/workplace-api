using AutoMapper;
using Microsoft.Extensions.Logging;
using workplace.Application.Dtos;
using workplace.Application.Interfaces;
using workplace.Domain.Entities;
using workplace.Domain.Interfaces;
using workplace.Infrastructure.Interfaces;

namespace workplace.Application.Services;

public class PlaceService: IPlaceService
{
    private readonly ILogger<PlaceService> _logger;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPlaceServiceInfrastructure _placeService;
    private readonly IMapper _mapper;

    public PlaceService(ILogger<PlaceService> logger, IPlaceRepository placeRepository, IPlaceServiceInfrastructure placeService, IMapper mapper)
    {
        _logger = logger;
        _placeRepository = placeRepository;
        _placeService = placeService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Crear espacio 
    /// </summary>
    public async Task<PlaceResponseDto?> CreatePlaceAsync(PlaceCreateDto placeCreateDto, Stream posterStream, Stream videoStream)
    {
        if (placeCreateDto == null)
        {
            _logger.LogWarning("El cuerpo de la peticion no puede estar vacio");
            throw new ArgumentNullException(nameof(placeCreateDto), "Datos de espacio requeridos");
        }

        if (placeCreateDto.posterFileName == null)
        {
            _logger.LogWarning("El nombre del archivo es un campo requerido");
            throw new ArgumentNullException(nameof(placeCreateDto.posterFileName),
                "El nombre del archivo es un campo requerido");
        }

        if (posterStream == null || posterStream.Length == 0)
        {
            _logger.LogWarning("El poster del epacio es requerido");
            throw new ArgumentNullException(nameof(posterStream), "El poster del epacio es requerido");
        }
        
        if (videoStream == null || videoStream.Length == 0)
        {
            _logger.LogWarning("El video del epacio es requerido");
            throw new ArgumentNullException(nameof(videoStream), "El video del epacio es requerido");
        }

        var posterUri = await _placeService.UploadPosterAsync(posterStream, placeCreateDto.posterFileName);
        var videoUri = await _placeService.UploadVideoAsync(videoStream, placeCreateDto.videoFileName);

        var newPlace = _mapper.Map<Place>(placeCreateDto);
        newPlace.ImagesUrl = posterUri;
        newPlace.VideosUrl = videoUri;
        

        return _mapper.Map<PlaceResponseDto>(await _placeRepository.CreatePlaceAsync(newPlace));
    }

    /// <summary>
    /// Obtener todos los espacios
    /// </summary>
    public async Task<IEnumerable<PlaceResponseDto?>> GetAllPlacesAsync()
    {
        var places =  await _placeRepository.GetAllPlacesAsync();
        return places.Select(place => _mapper.Map<PlaceResponseDto>(place));
    }

    /// <summary>
    /// Obtener un espacio
    /// </summary>
    public async Task<PlaceResponseDto?> GetPlaceByIdAsync(string placeId)
    {
        var place = await _placeRepository.GetPlaceByIdAsync(placeId);

        if (place == null)
        {
            _logger.LogWarning($"El espacio con id: {placeId} no se encuentra registrado");
            throw new KeyNotFoundException($"El espacio con id: {placeId} no se encuentra registrado");
        }
        
        return _mapper.Map<PlaceResponseDto>(place);
    }

    /// <summary>
    /// Eliminar espacio
    /// </summary>
    public async Task<bool> DeletePlaceAsync(string placeId)
    {
        var place = await _placeRepository.GetPlaceByIdAsync(placeId);

        if (place == null)
        {
            _logger.LogWarning($"El espacio con id: {placeId} no se encuentra registrado");
            throw new KeyNotFoundException($"El espacio con id: {placeId} no se encuentra registrado");
        }

        return await _placeRepository.DeletePlaceAsync(placeId);
    }
}
