using Azure;
using Microsoft.WindowsAzure.Storage.Table;
using workplace.Domain.Enums;
using ITableEntity = Azure.Data.Tables.ITableEntity;

namespace workplace.Domain.Entities;

public class User : ITableEntity
{
    public string PartitionKey { get; set; } = "USER";
    public string RowKey { get; set; } = string.Empty;
    public string Id { get; set; }= Guid.NewGuid().ToString();
    
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpire { get; set; }
    public string RoleValue { get; set; }
    
    [IgnoreProperty]
    public Role Role
    {
        get => Enum.TryParse<Role>(RoleValue, true, out var role) ? role : Role.Client;
        set => RoleValue = value.ToString();
    }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}