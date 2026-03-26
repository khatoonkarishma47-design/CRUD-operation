using ProductService.Models;

namespace ProductService.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<User?> RegisterAsync(RegisterRequest request);
    Task<User?> GetUserByUsernameAsync(string username);
}
