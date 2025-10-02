using backend.Dtos;
using backend.Models;

namespace backend.Services.IServices
{
    public interface IModeleService
    {
        Task<IEnumerable<ModeleResponseDto>> GetAllModelesAsync();
        Task<ModeleResponseDto?> GetModeleByIdAsync(int id);
        Task<ModeleResponseDto> CreateModeleAsync(CreateModeleDto createDto, IFormFile imageFile);
        Task<ModeleResponseDto?> UpdateModeleAsync(int id, UpdateModeleDto updateDto, IFormFile? imageFile = null);
        Task<bool> DeleteModeleAsync(int id);
    }
}