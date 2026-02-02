using Azure;
using Azure.Data.Tables;

namespace workplace.Domain.Entities;

public class Place : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public ICollection<string> ImagesUrl { get; set; } = new List<string>();
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}