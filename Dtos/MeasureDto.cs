using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class CreateMeasureDto
    {
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
    }

    public class UpdateMeasureDto
    {
        public decimal? TourPoitrine { get; set; }
        public decimal? TourHanches { get; set; }
        public decimal? LongueurManche { get; set; }
        public decimal? TourBras { get; set; }
        public decimal? LongueurChemise { get; set; }
        public decimal? LongueurPantalon { get; set; }
        public decimal? LargeurEpaules { get; set; }
        public decimal? TourCou { get; set; }
    }

    public class MeasureResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal? TourPoitrine { get; set; }
        public decimal? TourHanches { get; set; }
        public decimal? LongueurManche { get; set; }
        public decimal? TourBras { get; set; }
        public decimal? LongueurChemise { get; set; }
        public decimal? LongueurPantalon { get; set; }
        public decimal? LargeurEpaules { get; set; }
        public decimal? TourCou { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}