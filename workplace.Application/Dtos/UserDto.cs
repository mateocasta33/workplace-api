using Azure;
using Microsoft.WindowsAzure.Storage.Table;
using workplace.Domain.Enums;

namespace workplace.Application.Dtos;

public class UserResponseDto
{
    public string PartitionKey { get; set; } = "USER";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    
    public string Name { get; set; }
    public string Email { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpire { get; set; }
    public string RoleValue { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

public class UserLoginResponseDto
{
    public string PartitionKey { get; set; } = "USER";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpire { get; set; }
    public string RoleValue { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

public class UserRegisterRequestDto
{
    public string name { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public string role { get; set; }
}


public class UserLoginRequestDto
{
    public string email { get; set; }
    public string password { get; set; }
}
