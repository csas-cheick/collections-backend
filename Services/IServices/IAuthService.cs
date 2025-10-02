using backend.Dtos;

namespace backend.Services.IServices
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    }
}