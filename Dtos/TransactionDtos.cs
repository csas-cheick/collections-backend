using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    // DTO pour créer une nouvelle transaction
    public class CreateTransactionDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }

        [Required]
        [RegularExpression("^(ENTREE|SORTIE)$", ErrorMessage = "Le type doit être ENTREE ou SORTIE")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "La description est requise et ne peut pas dépasser 500 caractères")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Categorie { get; set; }

        [RegularExpression("^(ESPECES|CARTE|VIREMENT|CHEQUE)$", ErrorMessage = "Mode de paiement invalide")]
        public string? ModePaiement { get; set; }

        public DateTime? DateTransaction { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    // DTO pour mettre à jour une transaction
    public class UpdateTransactionDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal? Montant { get; set; }

        [RegularExpression("^(ENTREE|SORTIE)$", ErrorMessage = "Le type doit être ENTREE ou SORTIE")]
        public string? Type { get; set; }

        [StringLength(500, MinimumLength = 1, ErrorMessage = "La description ne peut pas être vide et ne peut pas dépasser 500 caractères")]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Categorie { get; set; }

        [RegularExpression("^(ESPECES|CARTE|VIREMENT|CHEQUE)$", ErrorMessage = "Mode de paiement invalide")]
        public string? ModePaiement { get; set; }

        public DateTime? DateTransaction { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    // DTO pour la réponse des transactions
    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public decimal Montant { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Categorie { get; set; }
        public string? ModePaiement { get; set; }
        public DateTime DateTransaction { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UserId { get; set; }
        public string? Notes { get; set; }
        public decimal MontantAvecSigne { get; set; }
    }

    // DTO pour les statistiques de caisse
    public class StatistiquesCaisseDto
    {
        public decimal TotalEntrees { get; set; }
        public decimal TotalSorties { get; set; }
        public decimal Solde { get; set; }
        public int NombreTransactions { get; set; }
        public DateTime PeriodeDebut { get; set; }
        public DateTime PeriodeFin { get; set; }
    }

    // DTO pour les filtres de recherche
    public class TransactionFiltersDto
    {
        public string? Type { get; set; } // "ENTREE", "SORTIE", ou null pour tous
        public string? Categorie { get; set; }
        public string? ModePaiement { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public decimal? MontantMin { get; set; }
        public decimal? MontantMax { get; set; }
        public string? Recherche { get; set; } // Recherche dans description et notes
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "DateTransaction"; // Champ de tri
        public string? SortOrder { get; set; } = "desc"; // "asc" ou "desc"
    }

    // DTO pour les transactions groupées par semaine
    public class TransactionsParSemaineDto
    {
        public int Annee { get; set; }
        public int NumeroSemaine { get; set; }
        public DateTime DebutSemaine { get; set; }
        public DateTime FinSemaine { get; set; }
        public List<TransactionResponseDto> Transactions { get; set; } = new List<TransactionResponseDto>();
        public TotauxSemaineDto Totaux { get; set; } = new TotauxSemaineDto();
    }

    // DTO pour les totaux d'une semaine
    public class TotauxSemaineDto
    {
        public decimal TotalEntrees { get; set; }
        public decimal TotalSorties { get; set; }
        public decimal SoldeNet { get; set; }
        public int NombreTransactions { get; set; }
        public int NombreEntrees { get; set; }
        public int NombreSorties { get; set; }
    }

    // DTO pour la réponse complète des transactions par semaine
    public class TransactionsGroupeesParSemaineResponseDto
    {
        public List<TransactionsParSemaineDto> Semaines { get; set; } = new List<TransactionsParSemaineDto>();
        public TotauxGenerauxDto TotauxGeneraux { get; set; } = new TotauxGenerauxDto();
    }

    // DTO pour les totaux généraux sur toute la période
    public class TotauxGenerauxDto
    {
        public decimal TotalEntreesGenerales { get; set; }
        public decimal TotalSortiesGenerales { get; set; }
        public decimal SoldeNetGeneral { get; set; }
        public int NombreTransactionsTotal { get; set; }
        public int NombreSemaines { get; set; }
        public DateTime PeriodeDebut { get; set; }
        public DateTime PeriodeFin { get; set; }
    }
}