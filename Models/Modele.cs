using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Modele
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix doit être supérieur à 0")]
        public decimal Price { get; set; }
        
        [Required]
        public string? ImageUrl { get; set; }
        
        public string? ImagePublicId { get; set; } // Pour Cloudinary
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}