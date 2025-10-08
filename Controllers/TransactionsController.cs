using backend.Dtos;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Récupérer toutes les transactions avec filtres optionnels
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetAllTransactions([FromQuery] TransactionFiltersDto? filters = null)
        {
            try
            {
                var transactions = await _transactionService.GetAllTransactionsAsync(filters);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des transactions", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer une transaction par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionResponseDto>> GetTransactionById(int id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound(new { message = "Transaction non trouvée" });
                }
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération de la transaction", error = ex.Message });
            }
        }

        /// <summary>
        /// Créer une nouvelle transaction
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TransactionResponseDto>> CreateTransaction([FromBody] CreateTransactionDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var transaction = await _transactionService.CreateTransactionAsync(createDto);
                return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la création de la transaction", error = ex.Message });
            }
        }

        /// <summary>
        /// Mettre à jour une transaction
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TransactionResponseDto>> UpdateTransaction(int id, [FromBody] UpdateTransactionDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var transaction = await _transactionService.UpdateTransactionAsync(id, updateDto);
                if (transaction == null)
                {
                    return NotFound(new { message = "Transaction non trouvée" });
                }

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la mise à jour de la transaction", error = ex.Message });
            }
        }

        /// <summary>
        /// Supprimer une transaction
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTransaction(int id)
        {
            try
            {
                var success = await _transactionService.DeleteTransactionAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Transaction non trouvée" });
                }

                return Ok(new { message = "Transaction supprimée avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la suppression de la transaction", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer les statistiques de caisse
        /// </summary>
        [HttpGet("statistiques")]
        public async Task<ActionResult<StatistiquesCaisseDto>> GetStatistiques([FromQuery] DateTime? dateDebut = null, [FromQuery] DateTime? dateFin = null)
        {
            try
            {
                var statistiques = await _transactionService.GetStatistiquesAsync(dateDebut, dateFin);
                return Ok(statistiques);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des statistiques", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer la liste des catégories utilisées
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                var categories = await _transactionService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des catégories", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer les transactions groupées par semaine
        /// </summary>
        [HttpGet("par-semaine")]
        public async Task<ActionResult<TransactionsGroupeesParSemaineResponseDto>> GetTransactionsParSemaine([FromQuery] DateTime? dateDebut = null, [FromQuery] DateTime? dateFin = null)
        {
            try
            {
                var transactionsGroupees = await _transactionService.GetTransactionsGroupeesParSemaineAsync(dateDebut, dateFin);
                return Ok(transactionsGroupees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la récupération des transactions par semaine", error = ex.Message });
            }
        }
    }
}