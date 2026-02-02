using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using workplace.Application.Dtos;
using workplace.Application.Interfaces;
using workplace.Domain.Entities;
using workplace.Domain.Interfaces;
using workplace.Infrastructure.Interfaces;

namespace workplace.Application.Services;

public class UserService: IUserService
{

    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly IPasswordService _passwordService;

    public UserService(
        IUserRepository repository,
        ILogger<UserService> logger,
        IMapper mapper,
        IConfiguration config,
        IPasswordService passwordService)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        _config = config;
        _passwordService = passwordService;
    }
    
    /// <summary>
    /// Crear usuarios
    /// </summary>
    public async Task<UserResponseDto?> CreateUser(UserRegisterRequestDto user)
    {
        
        if (user == null)
        {
            _logger.LogInformation("El cuerpo de la peticion no puede estar vacio");
            throw new ArgumentNullException("El cuerpo de la peticion no puede estar vacio");
        }
        
        var exist = await _repository.GetUserByEmailAsync(user.email);

        if (exist != null)
        {
            _logger.LogInformation($"El usuario con el email: {user.email} ya se encuentra registrado");
            throw new ArgumentException($"El usuario con el email: {user.email} ya se encuentra registrado");
        }

        var newUser = _mapper.Map<User>(user);
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.password);

        return  _mapper.Map<UserResponseDto>(await _repository.CreateUserAsync(newUser));
    }

    /// <summary>
    /// Login usuarios
    /// </summary>
    public async Task<UserLoginResponseDto?> LoginUser(UserLoginRequestDto user)
    {
        var exist = await _repository.GetUserByEmailAsync(user.email);

        if (exist == null || !_passwordService.Verify(user.password, exist.PasswordHash))
        {
            _logger.LogInformation("Credenciales invalidas");
            throw new SecurityException("Credenciales invalidas");
        }

        var response = _mapper.Map<UserLoginResponseDto>(exist);
        response.Token = GenerateToken(exist);

        return response;
    }

    public async Task<UserResponseDto?> UpdateUser(UserRegisterRequestDto user)
    {
        var exist = await _repository.GetUserByEmailAsync(user.email);

        if (exist == null)
        {
            _logger.LogInformation($"Usuario con email: {user.email} no encontrado");
            throw new KeyNotFoundException($"Usuario con email: {user.email} no encontrado");
        }

        var userupdate = await _repository.UpdateUserAsync( _mapper.Map<User>(user), exist.ETag);
        _logger.LogInformation("Usuario actualizado de forma exitosa");

        return _mapper.Map<UserResponseDto>(userupdate);
    }

    public async Task<bool> DeleteUser(string email)
    {
        var exist = await _repository.GetUserByEmailAsync(email);

        if (exist == null)
        {
            _logger.LogInformation($"Usuario con email: {email} no encontrado");
            throw new KeyNotFoundException($"Usuario con email: {email} no encontrado");
        }

        return await _repository.DeleteUserAsync(exist);
    }

    public async Task<IEnumerable<UserResponseDto?>> GetAllUsers()
    {
        var users = await _repository.GetAllUsersAsync();
        return users.Select(user => _mapper.Map<UserResponseDto>(user));
    }

    public async Task<UserResponseDto?> GetUserById(string id)
    {

        if (!Guid.TryParse(id, out var userId))
        {
            _logger.LogError("Id inválido");
            throw new  ArgumentException("Id inválido");
        }
        
        var exist = await _repository.GetUserByIdAsync(userId);
        
        if (exist == null)
        {
            _logger.LogInformation($"Usuario con id: {id} no encontrado");
            throw new KeyNotFoundException($"Usuario con id: {id} no encontrado");
        }
        
        _logger.LogInformation("Usuario encontrado de forma exitosa");
        return _mapper.Map<UserResponseDto>(exist);
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.RowKey),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.RoleValue)
        };

        var jwt = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            signingCredentials: credentials,
            expires: DateTime.UtcNow.AddHours(10)
            );


        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private string GenerateRefreshToken()
    {
        var array = new byte[36];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(array);
        return Convert.ToBase64String(array);
    }

    private ClaimsPrincipal GetClaimFromExpireToken(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
            ValidateLifetime = false
        };

        var handler = new JwtSecurityTokenHandler();

        var principal = handler.ValidateToken(token, parameters, out var SecurityToken);

        if (SecurityToken is not JwtSecurityToken securityToken ||
            !securityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
        {
            _logger.LogError("Token invalido");
            throw new SecurityException("Token invalido");
        }

        return principal;
    }
}