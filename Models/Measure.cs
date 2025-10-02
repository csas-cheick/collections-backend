using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Measure
    {
        [Key]
        public int Id { get; set; }
        // Clé étrangère vers Customer
        [Required]
        public int CustomerId { get; set; }
        public decimal? TourPoitrine { get; set; }
        public decimal? TourHanches { get; set; }        
        public decimal? LongueurManche { get; set; }
        public decimal? TourBras { get; set; }        
        public decimal? LongueurChemise { get; set; }
        public decimal? LongueurPantalon { get; set; }
        public decimal? LargeurEpaules { get; set; }
        public decimal? TourCou { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // Navigation property vers Customer
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;
    }
}