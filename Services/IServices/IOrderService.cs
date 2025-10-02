using backend.Dtos;

namespace backend.Services.IServices
{
    public interface IOrderService
    {
        // Gestion des commandes
        Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync();
        Task<OrderResponseDto?> GetOrderByIdAsync(int id);
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto createDto);
        Task<OrderResponseDto?> UpdateOrderAsync(int id, UpdateOrderDto updateDto);
        Task<bool> DeleteOrderAsync(int id);

        // Méthodes de calcul
        Task<decimal> CalculateOrderTotalAsync(List<CreateOrderItemDto> items);
        Task<decimal> CalculateFinalTotalAsync(decimal total, decimal? reduction);

        // Gestion des items de commande
        Task<OrderItemDto?> GetOrderItemByIdAsync(int id);
        Task<OrderItemDto> AddOrderItemAsync(int orderId, CreateOrderItemDto itemDto);
        Task<OrderItemDto?> UpdateOrderItemAsync(int id, UpdateOrderItemDto itemDto);
        Task<bool> DeleteOrderItemAsync(int id);

        // Méthodes utilitaires
        Task<IEnumerable<OrderSummaryDto>> GetOrdersByCustomerIdAsync(int customerId);
        Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(string status);
        Task<bool> UpdateOrderStatusAsync(int id, string status);

        // Gestion du calendrier des rendez-vous
        Task<IEnumerable<OrderSummaryDto>> GetOrdersWithAppointmentsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}