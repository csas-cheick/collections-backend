using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        // ModeleId est nullable pour permettre les modèles personnalisés
        public int? ModeleId { get; set; }

        // Champs pour les modèles personnalisés
        public bool IsCustomModel { get; set; } = false;

        [MaxLength(200)]
        public string? CustomModelName { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? CustomModelPrice { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeTissu { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Couleur { get; set; } = string.Empty;

        [Column(TypeName = "decimal(8,2)")]
        public decimal PrixUnitaire { get; set; }

        public int Quantite { get; set; } = 1;

        [MaxLength(300)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ModeleId")]
        public virtual Modele? Modele { get; set; }
    }
}