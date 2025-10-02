using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using backend.Services.IServices;

namespace backend.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
            var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
            var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("Configuration Cloudinary manquante");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "modeles")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Fichier invalide");

            // Vérifier le type de fichier
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Type de fichier non supporté. Utilisez JPEG, PNG, GIF ou WebP");

            // Taille maximale : 10MB
            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("La taille du fichier ne doit pas dépasser 10MB");

            try
            {
                using var stream = file.OpenReadStream();
                
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    Transformation = new Transformation()
                        .Quality("auto")
                        .FetchFormat("auto")
                        .Width(800)
                        .Height(600)
                        .Crop("limit"), // Limite la taille sans déformer
                    PublicId = $"{folder}_{Guid.NewGuid()}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Erreur lors de l'upload: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'upload sur Cloudinary: {ex.Message}");
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return false;

            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                // Log l'erreur mais ne pas faire échouer l'opération
                Console.WriteLine($"Erreur lors de la suppression sur Cloudinary: {ex.Message}");
                return false;
            }
        }

        // Méthode helper pour extraire le publicId d'une URL Cloudinary
        public string ExtractPublicIdFromUrl(string imageUrl)
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