using Azure;

namespace workplace.Application.Dtos;

public class PlaceResponseDto
{
    public string PartitionKey { get; set; } = "PLACE";
    public string RowKey { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public string ImagesUrl { get; set; }
    public string VideosUrl { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

public class PlaceCreateDto
{
    public string name { get; set; }
    public string description { get; set; }
    public int capacity { get; set; }
    public bool isActive { get; set; }
    
    public string imagesUrl { get; set; }
    public string videosUrl { get; set; }
    public string? posterFileName { get; set; }
    public string? videoFileName { get; set; }
    
}