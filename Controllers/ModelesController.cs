using backend.Dtos;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelesController : ControllerBase
    {
        private readonly IModeleService _modeleService;

        public ModelesController(IModeleService modeleService)
        {
            _modeleService = modeleService;
        }

        /// <summary>
        /// Récupérer tous les modèles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModeleResponseDto>>> GetAllModeles()
        {
            try
            {
                var modeles = await _modeleService.GetAllModelesAsync();
                return Ok(modeles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erreur: {ex.Message}" });
            }
        }

        /// <summary>
        /// Récupérer un modèle par ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ModeleResponseDto>> GetModeleById(int id)
        {
            try
            {
                var modele = await _modeleService.GetModeleByIdAsync(id);
                
                if (modele == null)
                    return NotFound(new { message = "Modèle non trouvé" });

                return Ok(modele);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erreur: {ex.Message}" });
            }
        }

        /// <summary>
        /// Créer un nouveau modèle avec image
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ModeleResponseDto>> CreateModele([FromForm] CreateModeleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createDto = new CreateModeleDto
                {
                    Price = request.Price
                };

                var modele = await _modeleService.CreateModeleAsync(createDto, request.ImageFile);
                return CreatedAtAction(nameof(GetModeleById), new { id = modele.Id }, modele);
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
        /// Mettre à jour un modèle
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ModeleResponseDto>> UpdateModele(int id, [FromForm] UpdateModeleRequest request)
        {
            try
            {
                var updateDto = new UpdateModeleDto
                {
                    Price = request.Price
                };

                var modele = await _modeleService.UpdateModeleAsync(id, updateDto, request.ImageFile);
                
                if (modele == null)
                    return NotFound(new { message = "Modèle non trouvé" });

                return Ok(modele);
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
        /// Supprimer un modèle
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteModele(int id)
        {
            try
            {
                var result = await _modeleService.DeleteModeleAsync(id);
                
                if (!result)
                    return NotFound(new { message = "Modèle non trouvé" });

                return Ok(new { message = "Modèle supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erreur interne: {ex.Message}" });
            }
        }
    }

    // Classes pour les requêtes avec fichiers
    public class CreateModeleRequest
    {
        public decimal Price { get; set; }
        public IFormFile ImageFile { get; set; } = null!;
    }

    public class UpdateModeleRequest
    {
        public decimal? Price { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}