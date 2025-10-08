using backend.Dtos;

namespace backend.Services.IServices
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionResponseDto>> GetAllTransactionsAsync(TransactionFiltersDto? filters = null);
        Task<TransactionResponseDto?> GetTransactionByIdAsync(int id);
        Task<TransactionResponseDto> CreateTransactionAsync(CreateTransactionDto createDto);
        Task<TransactionResponseDto?> UpdateTransactionAsync(int id, UpdateTransactionDto updateDto);
        Task<bool> DeleteTransactionAsync(int id);
        Task<StatistiquesCaisseDto> GetStatistiquesAsync(DateTime? dateDebut = null, DateTime? dateFin = null);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<TransactionsGroupeesParSemaineResponseDto> GetTransactionsGroupeesParSemaineAsync(DateTime? dateDebut = null, DateTime? dateFin = null);
    }
}