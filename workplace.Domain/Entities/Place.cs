using Azure;
using Azure.Data.Tables;

namespace workplace.Domain.Entities;

public class Place : ITableEntity
{
    public string PartitionKey { get; set; } = "PLACE";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Description { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }

    public string ImagesUrl { get; set; } = string.Empty;
    public string VideosUrl { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}