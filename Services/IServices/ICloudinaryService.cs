using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace backend.Services.IServices
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "modeles");
        Task<bool> DeleteImageAsync(string publicId);
    }
}