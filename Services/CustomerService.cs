using backend.Data;
using backend.Dtos;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public CustomerService(AppDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IEnumerable<CustomerSummaryDto>> GetAllCustomersAsync()
        {
            var customers = await _context.Customer
                .Include(c => c.Measure)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return customers.Select(c => new CustomerSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                PhotoUrl = c.PhotoUrl,
                CreatedAt = c.CreatedAt,
                HasMeasures = c.Measure != null
            });
        }

        public async Task<CustomerResponseDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customer
                .Include(c => c.Measure)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (customer == null)
                return null;

            return new CustomerResponseDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                PhotoUrl = customer.PhotoUrl,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                Measure = customer.Measure != null ? new MeasureResponseDto
                {
                    Id = customer.Measure.Id,
                    CustomerId = customer.Measure.CustomerId,
                    TourPoitrine = customer.Measure.TourPoitrine,
                    TourHanches = customer.Measure.TourHanches,
                    LongueurManche = customer.Measure.LongueurManche,
                    TourBras = customer.Measure.TourBras,
                    LongueurChemise = customer.Measure.LongueurChemise,
                    LongueurPantalon = customer.Measure.LongueurPantalon,
                    LargeurEpaules = customer.Measure.LargeurEpaules,
                    TourCou = customer.Measure.TourCou,
                    CreatedAt = customer.Measure.CreatedAt,
                    UpdatedAt = customer.Measure.UpdatedAt
                } : null
            };
        }

        public async Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto createDto, IFormFile? photoFile = null)
        {
            // Vérifier l'unicité du numéro
            var existingCustomer = await _context.Customer
                .FirstOrDefaultAsync(c => c.PhoneNumber == createDto.PhoneNumber);
            
            if (existingCustomer != null)
                throw new ArgumentException("Un client avec ce numéro existe déjà");

            try
            {
                string? photoUrl = null;

                // Upload de la photo si fournie
                if (photoFile != null && photoFile.Length > 0)
                {
                    photoUrl = await _cloudinaryService.UploadImageAsync(photoFile, "customers");
                }

                // Création du client
                var customer = new Customer
                {
                    Name = createDto.Name,
                    PhoneNumber = createDto.PhoneNumber,
                    PhotoUrl = photoUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Customer.Add(customer);
                await _context.SaveChangesAsync();

                return new CustomerResponseDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    PhoneNumber = customer.PhoneNumber,
                    PhotoUrl = customer.PhotoUrl,
                    CreatedAt = customer.CreatedAt,
                    UpdatedAt = customer.UpdatedAt,
                    Measure = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la création du client: {ex.Message}");
            }
        }

        public async Task<CustomerResponseDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateDto, IFormFile? photoFile = null)
        {
            var customer = await _context.Customer
                .Include(c => c.Measure)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (customer == null)
                return null;

            try
            {
                // Vérifier l'unicité du numéro si modifié
                if (!string.IsNullOrEmpty(updateDto.PhoneNumber) && updateDto.PhoneNumber != customer.PhoneNumber)
                {
                    var existingCustomer = await _context.Customer
                        .FirstOrDefaultAsync(c => c.PhoneNumber == updateDto.PhoneNumber && c.Id != id);
                    
                    if (existingCustomer != null)
                        throw new ArgumentException("Un client avec ce numéro existe déjà");
                    
                    customer.PhoneNumber = updateDto.PhoneNumber;
                }

                // Mise à jour des champs si fournis
                if (!string.IsNullOrEmpty(updateDto.Name))
                    customer.Name = updateDto.Name;

                // Upload d'une nouvelle photo si fournie
                if (photoFile != null && photoFile.Length > 0)
                {

                    var newPhotoUrl = await _cloudinaryService.UploadImageAsync(photoFile, "customers");
                    var newPublicId = ExtractPublicIdFromUrl(newPhotoUrl);

                    customer.PhotoUrl = newPhotoUrl;
                }

                customer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new CustomerResponseDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    PhoneNumber = customer.PhoneNumber,
                    PhotoUrl = customer.PhotoUrl,
                    CreatedAt = customer.CreatedAt,
                    UpdatedAt = customer.UpdatedAt,
                    Measure = customer.Measure != null ? new MeasureResponseDto
                    {
                        Id = customer.Measure.Id,
                        CustomerId = customer.Measure.CustomerId,
                        TourPoitrine = customer.Measure.TourPoitrine,
                        TourHanches = customer.Measure.TourHanches,
                        LongueurManche = customer.Measure.LongueurManche,
                        TourBras = customer.Measure.TourBras,
                        LongueurChemise = customer.Measure.LongueurChemise,
                        LongueurPantalon = customer.Measure.LongueurPantalon,
                        LargeurEpaules = customer.Measure.LargeurEpaules,
                        TourCou = customer.Measure.TourCou,
                        CreatedAt = customer.Measure.CreatedAt,
                        UpdatedAt = customer.Measure.UpdatedAt
                    } : null
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la mise à jour du client: {ex.Message}");
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customer
                .Include(c => c.Measure)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (customer == null)
                return false;

            try
            {
                _context.Customer.Remove(customer);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la suppression du client: {ex.Message}");
            }
        }

        public async Task<MeasureResponseDto?> GetCustomerMeasuresAsync(int customerId)
        {
            var measure = await _context.Measure
                .FirstOrDefaultAsync(m => m.CustomerId == customerId);
            
            if (measure == null)
                return null;

            return new MeasureResponseDto
            {
                Id = measure.Id,
                CustomerId = measure.CustomerId,
                TourPoitrine = measure.TourPoitrine,
                TourHanches = measure.TourHanches,
                LongueurManche = measure.LongueurManche,
                TourBras = measure.TourBras,
                LongueurChemise = measure.LongueurChemise,
                LongueurPantalon = measure.LongueurPantalon,
                LargeurEpaules = measure.LargeurEpaules,
                TourCou = measure.TourCou,
                CreatedAt = measure.CreatedAt,
                UpdatedAt = measure.UpdatedAt
            };
        }

        public async Task<MeasureResponseDto> CreateOrUpdateMeasuresAsync(int customerId, CreateMeasureDto measureDto)
        {
            // Vérifier que le client existe
            var customer = await _context.Customer.FindAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Client non trouvé");

            try
            {
                var existingMeasure = await _context.Measure
                    .FirstOrDefaultAsync(m => m.CustomerId == customerId);

                if (existingMeasure != null)
                {
                    // Mise à jour des mesures existantes
                    existingMeasure.TourPoitrine = measureDto.TourPoitrine;
                    existingMeasure.TourHanches = measureDto.TourHanches;
                    existingMeasure.LongueurManche = measureDto.LongueurManche;
                    existingMeasure.TourBras = measureDto.TourBras;
                    existingMeasure.LongueurChemise = measureDto.LongueurChemise;
                    existingMeasure.LongueurPantalon = measureDto.LongueurPantalon;
                    existingMeasure.LargeurEpaules = measureDto.LargeurEpaules;
                    existingMeasure.TourCou = measureDto.TourCou;
                    existingMeasure.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return new MeasureResponseDto
                    {
                        Id = existingMeasure.Id,
                        CustomerId = existingMeasure.CustomerId,
                        TourPoitrine = existingMeasure.TourPoitrine,
                        TourHanches = existingMeasure.TourHanches,
                        LongueurManche = existingMeasure.LongueurManche,
                        TourBras = existingMeasure.TourBras,
                        LongueurChemise = existingMeasure.LongueurChemise,
                        LongueurPantalon = existingMeasure.LongueurPantalon,
                        LargeurEpaules = existingMeasure.LargeurEpaules,
                        TourCou = existingMeasure.TourCou,
                        CreatedAt = existingMeasure.CreatedAt,
                        UpdatedAt = existingMeasure.UpdatedAt
                    };
                }
                else
                {
                    // Création de nouvelles mesures
                    var newMeasure = new Measure
                    {
                        CustomerId = customerId,
                        TourPoitrine = measureDto.TourPoitrine,
                        TourHanches = measureDto.TourHanches,
                        LongueurManche = measureDto.LongueurManche,
                        TourBras = measureDto.TourBras,
                        LongueurChemise = measureDto.LongueurChemise,
                        LongueurPantalon = measureDto.LongueurPantalon,
                        LargeurEpaules = measureDto.LargeurEpaules,
                        TourCou = measureDto.TourCou,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Measure.Add(newMeasure);
                    await _context.SaveChangesAsync();

                    return new MeasureResponseDto
                    {
                        Id = newMeasure.Id,
                        CustomerId = newMeasure.CustomerId,
                        TourPoitrine = newMeasure.TourPoitrine,
                        TourHanches = newMeasure.TourHanches,
                        LongueurManche = newMeasure.LongueurManche,
                        TourBras = newMeasure.TourBras,
                        LongueurChemise = newMeasure.LongueurChemise,
                        LongueurPantalon = newMeasure.LongueurPantalon,
                        LargeurEpaules = newMeasure.LargeurEpaules,
                        TourCou = newMeasure.TourCou,
                        CreatedAt = newMeasure.CreatedAt,
                        UpdatedAt = newMeasure.UpdatedAt
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la gestion des mesures: {ex.Message}");
            }
        }

        public async Task<bool> DeleteMeasuresAsync(int customerId)
        {
            var measure = await _context.Measure
                .FirstOrDefaultAsync(m => m.CustomerId == customerId);
            
            if (measure == null)
                return false;

            try
            {
                _context.Measure.Remove(measure);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la suppression des mesures: {ex.Message}");
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