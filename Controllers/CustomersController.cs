using backend.Dtos;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Récupérer tous les clients (résumé)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerSummaryDto>>> GetAllCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erreur: {ex.Message}" });
            }
        }

        /// <summary>
        /// Récupérer un client par ID avec ses mesures
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerResponseDto>> GetCustomerById(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                
                if (customer == null)
                    return NotFound(new { message = "Client non trouvé" });

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erreur: {ex.Message}" });
            }
        }

        /// <summary>
        /// Créer un nouveau client avec photo optionnelle
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CustomerResponseDto>> CreateCustomer([FromForm] CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createDto = new CreateCustomerDto
                {
                    Name = request.Name,
                    PhoneNumber = request.PhoneNumber
                };

                var customer = await _customerService.CreateCustomerAsync(createDto, request.PhotoFile);
                return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur interne: {ex.Message}" });
            }
        }

        /// <summary>
        /// Mettre à jour un client
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerResponseDto>> UpdateCustomer(int id, [FromForm] UpdateCustomerRequest request)
        {
            try
            {
                var updateDto = new UpdateCustomerDto
                {
                    Name = request.Name,
                    PhoneNumber = request.PhoneNumber
                };

                var customer = await _customerService.UpdateCustomerAsync(id, updateDto, request.PhotoFile);
                
                if (customer == null)
                    return NotFound(new { message = "Client non trouvé" });

                return Ok(customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur interne: {ex.Message}" });
            }
        }

        /// <summary>
        /// Supprimer un client
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                
                if (!result)
                    return NotFound(new { message = "Client non trouvé" });

                return Ok(new { message = "Client supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur interne: {ex.Message}" });
            }
        }

        /// <summary>
        /// Récupérer les mesures d'un client
        /// </summary>
        [HttpGet("{customerId}/measures")]
        public async Task<ActionResult<MeasureResponseDto>> GetCustomerMeasures(int customerId)
        {
            try
            {
                var measures = await _customerService.GetCustomerMeasuresAsync(customerId);
                
                if (measures == null)
                    return NotFound(new { message = "Mesures non trouvées pour ce client" });

                return Ok(measures);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erreur: {ex.Message}" });
            }
        }

        /// <summary>
        /// Créer ou mettre à jour les mesures d'un client
        /// </summary>
        [HttpPost("{customerId}/measures")]
        public async Task<ActionResult<MeasureResponseDto>> CreateOrUpdateMeasures(int customerId, [FromBody] CreateMeasureDto measureDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // S'assurer que l'ID du client correspond
                measureDto.CustomerId = customerId;
                
                var measures = await _customerService.CreateOrUpdateMeasuresAsync(customerId, measureDto);
                return Ok(measures);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur interne: {ex.Message}" });
            }
        }

        /// <summary>
        /// Supprimer les mesures d'un client
        /// </summary>
        [HttpDelete("{customerId}/measures")]
        public async Task<ActionResult> DeleteMeasures(int customerId)
        {
            try
            {
                var result = await _customerService.DeleteMeasuresAsync(customerId);
                
                if (!result)
                    return NotFound(new { message = "Mesures non trouvées pour ce client" });

                return Ok(new { message = "Mesures supprimées avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur interne: {ex.Message}" });
            }
        }
    }

    // Classes de requête pour les formulaires multipart
    public class CreateCustomerRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        public IFormFile? PhotoFile { get; set; }
    }

    public class UpdateCustomerRequest
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? PhotoFile { get; set; }
    }
}