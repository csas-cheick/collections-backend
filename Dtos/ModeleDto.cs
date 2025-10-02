using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class CreateModeleDto
    {
        [Required(ErrorMessage = "Le prix est requis")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix doit être supérieur à 0")]
        public decimal Price { get; set; }
    }

    public class UpdateModeleDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix doit être supérieur à 0")]
        public decimal? Price { get; set; }
    }

    public class ModeleResponseDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}