using workplace.Application.Dtos;

namespace workplace.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateUser(UserRegisterRequestDto user);
    Task<UserLoginResponseDto> LoginUser(UserLoginRequestDto user);
    Task<UserResponseDto> UpdateUser(UserRegisterRequestDto user);
    Task<bool> DeleteUser(string email);
    Task<IEnumerable<UserResponseDto?>> GetAllUsers();
    Task<UserResponseDto?> GetUserById(string id);
}