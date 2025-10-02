using backend.Dtos;
using backend.Models;

namespace backend.Services.IServices
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerSummaryDto>> GetAllCustomersAsync();
        Task<CustomerResponseDto?> GetCustomerByIdAsync(int id);
        Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto createDto, IFormFile? photoFile = null);
        Task<CustomerResponseDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateDto, IFormFile? photoFile = null);
        Task<bool> DeleteCustomerAsync(int id);
        
        // Méthodes pour les mesures
        Task<MeasureResponseDto?> GetCustomerMeasuresAsync(int customerId);
        Task<MeasureResponseDto> CreateOrUpdateMeasuresAsync(int customerId, CreateMeasureDto measureDto);
        Task<bool> DeleteMeasuresAsync(int customerId);
    }
}