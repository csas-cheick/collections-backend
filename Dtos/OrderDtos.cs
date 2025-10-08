using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    // DTOs pour OrderItem
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int? ModeleId { get; set; }
        public string? ModeleNom { get; set; }
        public bool IsCustomModel { get; set; }
        public string? CustomModelName { get; set; }
        public decimal? CustomModelPrice { get; set; }
        public string TypeTissu { get; set; } = string.Empty;
        public string Couleur { get; set; } = string.Empty;
        public decimal PrixUnitaire { get; set; }
        public int Quantite { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateOrderItemDto
    {
        public int? ModeleId { get; set; }

        public bool IsCustomModel { get; set; } = false;

        [MaxLength(200)]
        public string? CustomModelName { get; set; }

        public decimal? CustomModelPrice { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeTissu { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Couleur { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantite { get; set; } = 1;

        [MaxLength(300)]
        public string? Notes { get; set; }
    }

    public class UpdateOrderItemDto
    {
        public int? ModeleId { get; set; }

        [MaxLength(100)]
        public string? TypeTissu { get; set; }

        [MaxLength(100)]
        public string? Couleur { get; set; }

        [Range(1, int.MaxValue)]
        public int? Quantite { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }
    }

    // DTOs pour Order
    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime DateCommande { get; set; }
        public DateTime? DateRendezVous { get; set; }
        public decimal Total { get; set; }
        public decimal? Reduction { get; set; }
        public decimal TotalFinal { get; set; }
        public string Statut { get; set; } = string.Empty;
        public int NombreItems { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime DateCommande { get; set; }
        public DateTime? DateRendezVous { get; set; }
        public decimal Total { get; set; }
        public decimal? Reduction { get; set; }
        public decimal TotalFinal { get; set; }
        public string Statut { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateOrderDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public DateTime DateCommande { get; set; } = DateTime.UtcNow;

        public DateTime? DateRendezVous { get; set; }

        [MaxLength(20)]
        public string Statut { get; set; } = "En cours";

        [MaxLength(500)]
        public string? Notes { get; set; }

        public decimal? Reduction { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
    }

    public class UpdateOrderDto
    {
        public int? CustomerId { get; set; }
        public DateTime? DateCommande { get; set; }
        public DateTime? DateRendezVous { get; set; }

        [MaxLength(20)]
        public string? Statut { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public decimal? Reduction { get; set; }

        public List<CreateOrderItemDto>? OrderItems { get; set; }
    }
}