using backend.Data;
using backend.Dtos;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransactionResponseDto>> GetAllTransactionsAsync(TransactionFiltersDto? filters = null)
        {
            var query = _context.Transaction.AsQueryable();

            // Appliquer les filtres
            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.Type))
                    query = query.Where(t => t.Type == filters.Type);

                if (!string.IsNullOrEmpty(filters.Categorie))
                    query = query.Where(t => t.Categorie == filters.Categorie);

                if (!string.IsNullOrEmpty(filters.ModePaiement))
                    query = query.Where(t => t.ModePaiement == filters.ModePaiement);

                if (filters.DateDebut.HasValue)
                    query = query.Where(t => t.DateTransaction >= filters.DateDebut);

                if (filters.DateFin.HasValue)
                    query = query.Where(t => t.DateTransaction <= filters.DateFin);

                if (filters.MontantMin.HasValue)
                    query = query.Where(t => t.Montant >= filters.MontantMin);

                if (filters.MontantMax.HasValue)
                    query = query.Where(t => t.Montant <= filters.MontantMax);

                if (!string.IsNullOrEmpty(filters.Recherche))
                {
                    var recherche = filters.Recherche.ToLower();
                    query = query.Where(t => 
                        t.Description.ToLower().Contains(recherche) ||
                        (t.Notes != null && t.Notes.ToLower().Contains(recherche)));
                }

                // Tri
                switch (filters.SortBy?.ToLower())
                {
                    case "montant":
                        query = filters.SortOrder?.ToLower() == "asc" ? 
                            query.OrderBy(t => t.Montant) : 
                            query.OrderByDescending(t => t.Montant);
                        break;
                    case "type":
                        query = filters.SortOrder?.ToLower() == "asc" ? 
                            query.OrderBy(t => t.Type) : 
                            query.OrderByDescending(t => t.Type);
                        break;
                    case "description":
                        query = filters.SortOrder?.ToLower() == "asc" ? 
                            query.OrderBy(t => t.Description) : 
                            query.OrderByDescending(t => t.Description);
                        break;
                    default: // DateTransaction
                        query = filters.SortOrder?.ToLower() == "asc" ? 
                            query.OrderBy(t => t.DateTransaction) : 
                            query.OrderByDescending(t => t.DateTransaction);
                        break;
                }

                // Pagination
                if (filters.Page > 0 && filters.PageSize > 0)
                {
                    query = query.Skip((filters.Page - 1) * filters.PageSize)
                                 .Take(filters.PageSize);
                }
            }
            else
            {
                // Tri par défaut par date décroissante
                query = query.OrderByDescending(t => t.DateTransaction);
            }

            var transactions = await query.ToListAsync();

            return transactions.Select(MapToResponseDto);
        }

        public async Task<TransactionResponseDto?> GetTransactionByIdAsync(int id)
        {
            var transaction = await _context.Transaction.FindAsync(id);
            return transaction != null ? MapToResponseDto(transaction) : null;
        }

        public async Task<TransactionResponseDto> CreateTransactionAsync(CreateTransactionDto createDto)
        {
            var transaction = new Transaction
            {
                Montant = createDto.Montant,
                Type = createDto.Type,
                Description = createDto.Description,
                Categorie = createDto.Categorie,
                ModePaiement = createDto.ModePaiement,
                DateTransaction = createDto.DateTransaction ?? DateTime.Now,
                Notes = createDto.Notes,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Transaction.Add(transaction);
            await _context.SaveChangesAsync();

            return MapToResponseDto(transaction);
        }

        public async Task<TransactionResponseDto?> UpdateTransactionAsync(int id, UpdateTransactionDto updateDto)
        {
            var transaction = await _context.Transaction.FindAsync(id);
            if (transaction == null)
                return null;

            // Mettre à jour les champs modifiés
            if (updateDto.Montant.HasValue)
                transaction.Montant = updateDto.Montant.Value;

            if (!string.IsNullOrEmpty(updateDto.Type))
                transaction.Type = updateDto.Type;

            if (!string.IsNullOrEmpty(updateDto.Description))
                transaction.Description = updateDto.Description;

            if (updateDto.Categorie != null)
                transaction.Categorie = updateDto.Categorie;

            if (updateDto.ModePaiement != null)
                transaction.ModePaiement = updateDto.ModePaiement;

            if (updateDto.DateTransaction.HasValue)
                transaction.DateTransaction = updateDto.DateTransaction.Value;

            if (updateDto.Notes != null)
                transaction.Notes = updateDto.Notes;

            transaction.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return MapToResponseDto(transaction);
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transaction.FindAsync(id);
            if (transaction == null)
                return false;

            _context.Transaction.Remove(transaction);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<StatistiquesCaisseDto> GetStatistiquesAsync(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            var query = _context.Transaction.AsQueryable();

            // Filtrer par période si spécifiée
            if (dateDebut.HasValue)
                query = query.Where(t => t.DateTransaction >= dateDebut);

            if (dateFin.HasValue)
                query = query.Where(t => t.DateTransaction <= dateFin);

            var transactions = await query.ToListAsync();

            var totalEntrees = transactions.Where(t => t.Type == "ENTREE").Sum(t => t.Montant);
            var totalSorties = transactions.Where(t => t.Type == "SORTIE").Sum(t => t.Montant);

            return new StatistiquesCaisseDto
            {
                TotalEntrees = totalEntrees,
                TotalSorties = totalSorties,
                Solde = totalEntrees - totalSorties,
                NombreTransactions = transactions.Count,
                PeriodeDebut = dateDebut ?? transactions.Min(t => t?.DateTransaction) ?? DateTime.Now,
                PeriodeFin = dateFin ?? transactions.Max(t => t?.DateTransaction) ?? DateTime.Now
            };
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.Transaction
                .Where(t => !string.IsNullOrEmpty(t.Categorie))
                .Select(t => t.Categorie!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        private static TransactionResponseDto MapToResponseDto(Transaction transaction)
        {
            return new TransactionResponseDto
            {
                Id = transaction.Id,
                Montant = transaction.Montant,
                Type = transaction.Type,
                Description = transaction.Description,
                Categorie = transaction.Categorie,
                ModePaiement = transaction.ModePaiement,
                DateTransaction = transaction.DateTransaction,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt,
                UserId = transaction.UserId,
                Notes = transaction.Notes,
                MontantAvecSigne = transaction.MontantAvecSigne
            };
        }

        public async Task<TransactionsGroupeesParSemaineResponseDto> GetTransactionsGroupeesParSemaineAsync(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            // Si aucune date n'est spécifiée, prendre les 30 derniers jours
            var finPeriode = dateFin ?? DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            var debutPeriode = dateDebut ?? DateTime.Now.Date.AddDays(-30);

            // Récupérer toutes les transactions dans la période
            var transactions = await _context.Transaction
                .Where(t => t.DateTransaction >= debutPeriode && t.DateTransaction <= finPeriode)
                .OrderBy(t => t.DateTransaction)
                .ToListAsync();

            // Grouper par semaine
            var semainesGroupees = transactions
                .GroupBy(t => new
                {
                    Annee = GetWeekYear(t.DateTransaction),
                    NumeroSemaine = GetWeekOfYear(t.DateTransaction)
                })
                .Select(g => new TransactionsParSemaineDto
                {
                    Annee = g.Key.Annee,
                    NumeroSemaine = g.Key.NumeroSemaine,
                    DebutSemaine = GetFirstDayOfWeek(g.First().DateTransaction),
                    FinSemaine = GetLastDayOfWeek(g.First().DateTransaction),
                    Transactions = g.Select(MapToResponseDto).ToList(),
                    Totaux = CalculerTotauxSemaine(g.ToList())
                })
                .OrderBy(s => s.Annee)
                .ThenBy(s => s.NumeroSemaine)
                .ToList();

            // Calculer les totaux généraux
            var totauxGeneraux = new TotauxGenerauxDto
            {
                TotalEntreesGenerales = transactions.Where(t => t.Type == "ENTREE").Sum(t => t.Montant),
                TotalSortiesGenerales = transactions.Where(t => t.Type == "SORTIE").Sum(t => t.Montant),
                SoldeNetGeneral = transactions.Sum(t => t.MontantAvecSigne),
                NombreTransactionsTotal = transactions.Count,
                NombreSemaines = semainesGroupees.Count,
                PeriodeDebut = debutPeriode,
                PeriodeFin = finPeriode
            };

            return new TransactionsGroupeesParSemaineResponseDto
            {
                Semaines = semainesGroupees,
                TotauxGeneraux = totauxGeneraux
            };
        }

        private static int GetWeekOfYear(DateTime date)
        {
            // Utilise la norme ISO 8601 pour déterminer le numéro de semaine
            var day = (int)System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(date);
            if (day >= 0 && day <= 6)
                date = date.AddDays(3);

            return System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private static int GetWeekYear(DateTime date)
        {
            var weekNum = GetWeekOfYear(date);
            if (weekNum >= 52 && date.Month == 1)
                return date.Year - 1;
            if (weekNum == 1 && date.Month == 12)
                return date.Year + 1;
            return date.Year;
        }

        private static DateTime GetFirstDayOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        private static DateTime GetLastDayOfWeek(DateTime date)
        {
            return GetFirstDayOfWeek(date).AddDays(6);
        }

        private static TotauxSemaineDto CalculerTotauxSemaine(List<Transaction> transactions)
        {
            var entrees = transactions.Where(t => t.Type == "ENTREE").ToList();
            var sorties = transactions.Where(t => t.Type == "SORTIE").ToList();

            return new TotauxSemaineDto
            {
                TotalEntrees = entrees.Sum(t => t.Montant),
                TotalSorties = sorties.Sum(t => t.Montant),
                SoldeNet = transactions.Sum(t => t.MontantAvecSigne),
                NombreTransactions = transactions.Count,
                NombreEntrees = entrees.Count,
                NombreSorties = sorties.Count
            };
        }
    }
}