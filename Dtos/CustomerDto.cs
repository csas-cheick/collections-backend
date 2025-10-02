using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class CreateCustomerDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class UpdateCustomerDto
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class CustomerResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public MeasureResponseDto? Measure { get; set; }
    }

    public class CustomerSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasMeasures { get; set; }
    }
}