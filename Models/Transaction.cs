using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }

        [Required]
        [StringLength(10)]
        public string Type { get; set; } = string.Empty; // "ENTREE" ou "SORTIE"

        [Required]
        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Categorie { get; set; }

        [StringLength(50)]
        public string? ModePaiement { get; set; } // "ESPECES", "CARTE", "VIREMENT", "CHEQUE"

        public DateTime DateTransaction { get; set; } = DateTime.Now;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Optionnel : référence à l'utilisateur qui a créé la transaction
        public int? UserId { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Propriété calculée pour l'affichage
        public decimal MontantAvecSigne => Type == "ENTREE" ? Montant : -Montant;
    }
}