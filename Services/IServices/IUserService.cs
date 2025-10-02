using backend.Dtos;

namespace backend.Services.IServices
{
    public interface IUserService
    {
        Task<UserListResponseDto> GetAllUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null, bool? status = null);
        Task<UserOperationResponseDto> GetUserByIdAsync(int id);
        Task<UserOperationResponseDto> GetCurrentUserProfileAsync(int userId);
        Task<UserOperationResponseDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserOperationResponseDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<UserOperationResponseDto> DeleteUserAsync(int id);
        Task<UserOperationResponseDto> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto);
        Task<UserOperationResponseDto> ToggleUserStatusAsync(int id);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<bool> UserExistsByUserNameAsync(string userName);
    }
}