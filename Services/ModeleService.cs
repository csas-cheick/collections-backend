using backend.Data;
using backend.Dtos;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class ModeleService : IModeleService
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public ModeleService(AppDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IEnumerable<ModeleResponseDto>> GetAllModelesAsync()
        {
            var modeles = await _context.Modele
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return modeles.Select(m => new ModeleResponseDto
            {
                Id = m.Id,
                Price = m.Price,
                ImageUrl = m.ImageUrl,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            });
        }

        public async Task<ModeleResponseDto?> GetModeleByIdAsync(int id)
        {
            var modele = await _context.Modele.FindAsync(id);
            
            if (modele == null)
                return null;

            return new ModeleResponseDto
            {
                Id = modele.Id,
                Price = modele.Price,
                ImageUrl = modele.ImageUrl,
                CreatedAt = modele.CreatedAt,
                UpdatedAt = modele.UpdatedAt
            };
        }

        public async Task<ModeleResponseDto> CreateModeleAsync(CreateModeleDto createDto, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Une image est requise");

            try
            {
                // Upload de l'image sur Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(imageFile, "modeles");
                var publicId = ExtractPublicIdFromUrl(imageUrl);

                // Création du modèle
                var modele = new Modele
                {
                    Price = createDto.Price,
                    ImageUrl = imageUrl,
                    ImagePublicId = publicId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Modele.Add(modele);
                await _context.SaveChangesAsync();

                return new ModeleResponseDto
                {
                    Id = modele.Id,
                    Price = modele.Price,
                    ImageUrl = modele.ImageUrl,
                    CreatedAt = modele.CreatedAt,
                    UpdatedAt = modele.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la création du modèle: {ex.Message}");
            }
        }

        public async Task<ModeleResponseDto?> UpdateModeleAsync(int id, UpdateModeleDto updateDto, IFormFile? imageFile = null)
        {
            var modele = await _context.Modele.FindAsync(id);
            if (modele == null)
                return null;

            try
            {
                // Mise à jour du prix si fourni
                if (updateDto.Price.HasValue)
                {
                    modele.Price = updateDto.Price.Value;
                }

                // Upload d'une nouvelle image si fournie
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Supprimer l'ancienne image
                    if (!string.IsNullOrEmpty(modele.ImagePublicId))
                    {
                        await _cloudinaryService.DeleteImageAsync(modele.ImagePublicId);
                    }

                    // Upload de la nouvelle image
                    var newImageUrl = await _cloudinaryService.UploadImageAsync(imageFile, "modeles");
                    var newPublicId = ExtractPublicIdFromUrl(newImageUrl);

                    modele.ImageUrl = newImageUrl;
                    modele.ImagePublicId = newPublicId;
                }

                modele.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ModeleResponseDto
                {
                    Id = modele.Id,
                    Price = modele.Price,
                    ImageUrl = modele.ImageUrl,
                    CreatedAt = modele.CreatedAt,
                    UpdatedAt = modele.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la mise à jour du modèle: {ex.Message}");
            }
        }

        public async Task<bool> DeleteModeleAsync(int id)
        {
            var modele = await _context.Modele.FindAsync(id);
            if (modele == null)
                return false;

            try
            {
                // Supprimer l'image de Cloudinary
                if (!string.IsNullOrEmpty(modele.ImagePublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(modele.ImagePublicId);
                }

                // Supprimer le modèle de la base de données
                _context.Modele.Remove(modele);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la suppression du modèle: {ex.Message}");
            }
        }

        // Méthode helper pour extraire le publicId d'une URL
        private string ExtractPublicIdFromUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return string.Empty;

            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');
                
                // Trouve l'index du segment version (v1234567890)
                var versionIndex = Array.FindIndex(segments, s => s.StartsWith("v") && s.Length > 1);
                if (versionIndex > 0 && versionIndex < segments.Length - 1)
                {
                    // Combine tous les segments après la version
                    var pathSegments = segments.Skip(versionIndex + 1);
                    var publicIdWithExtension = string.Join("/", pathSegments);
                    
                    // Retire l'extension
                    var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
                    return lastDotIndex > 0 ? publicIdWithExtension.Substring(0, lastDotIndex) : publicIdWithExtension;
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}