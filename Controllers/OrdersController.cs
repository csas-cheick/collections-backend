using backend.Dtos;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Récupérer toutes les commandes (résumé)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer une commande par ID avec ses items
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Commande non trouvée" });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Créer une nouvelle commande
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Données invalides", errors = ModelState });
                }

                var order = await _orderService.CreateOrderAsync(createDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erreur lors de la création de la commande", details = ex.Message });
            }
        }

        /// <summary>
        /// Mettre à jour une commande existante
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderResponseDto>> UpdateOrder(int id, [FromBody] UpdateOrderDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Données invalides", errors = ModelState });
                }

                var order = await _orderService.UpdateOrderAsync(id, updateDto);
                if (order == null)
                {
                    return NotFound(new { success = false, message = "Commande non trouvée" });
                }

                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erreur lors de la mise à jour de la commande", details = ex.Message });
            }
        }

        /// <summary>
        /// Supprimer une commande
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Commande non trouvée" });
                }

                return Ok(new { success = true, message = "Commande supprimée avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erreur lors de la suppression de la commande", details = ex.Message });
            }
        }

        /// <summary>
        /// Calculer le total d'une commande
        /// </summary>
        [HttpPost("calculate-total")]
        public async Task<ActionResult<decimal>> CalculateOrderTotal([FromBody] List<CreateOrderItemDto> items)
        {
            try
            {
                var total = await _orderService.CalculateOrderTotalAsync(items);
                return Ok(new { total });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Calculer le total final avec réduction
        /// </summary>
        [HttpPost("calculate-final-total")]
        public async Task<ActionResult<decimal>> CalculateFinalTotal([FromBody] FinalTotalRequest request)
        {
            try
            {
                var finalTotal = await _orderService.CalculateFinalTotalAsync(request.Total, request.Reduction);
                return Ok(new { finalTotal });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer les commandes d'un client
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByCustomer(int customerId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer les commandes par statut
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mettre à jour le statut d'une commande
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Status))
                {
                    return BadRequest(new { success = false, message = "Le statut est requis" });
                }

                var result = await _orderService.UpdateOrderStatusAsync(id, request.Status);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Commande non trouvée" });
                }

                return Ok(new { success = true, message = "Statut mis à jour avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erreur lors de la mise à jour du statut", details = ex.Message });
            }
        }

        // ========== GESTION DES ITEMS DE COMMANDE ==========

        /// <summary>
        /// Récupérer un item de commande par ID
        /// </summary>
        [HttpGet("items/{id}")]
        public async Task<ActionResult<OrderItemDto>> GetOrderItemById(int id)
        {
            try
            {
                var item = await _orderService.GetOrderItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound(new { success = false, message = "Item de commande non trouvé" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ajouter un item à une commande existante
        /// </summary>
        [HttpPost("{orderId}/items")]
        public async Task<ActionResult<OrderItemDto>> AddOrderItem(int orderId, [FromBody] CreateOrderItemDto itemDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Données invalides", errors = ModelState });
                }

                var item = await _orderService.AddOrderItemAsync(orderId, itemDto);
                return CreatedAtAction(nameof(GetOrderItemById), new { id = item.Id }, item);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erreur lors de l'ajout de l'item", details = ex.Message });
            }
        }

        /// <summary>
        /// Mettre à jour un item de commande
        /// </summary>
        [HttpPut("items/{id}")]
        public async Task<ActionResult<OrderItemDto>> UpdateOrderItem(int id, [FromBody] UpdateOrderItemDto itemDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Données invalides", errors = ModelState });
                }

                var item = await _orderService.UpdateOrderItemAsync(id, itemDto);
                if (item == null)
                {
                    return NotFound(new { success = false, message = "Item de commande non trouvé" });
                }

                return Ok(item);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erreur lors de la mise à jour de l'item", details = ex.Message });
            }
        }

        /// <summary>
        /// Supprimer un item de commande
        /// </summary>
        [HttpDelete("items/{id}")]
        public async Task<ActionResult> DeleteOrderItem(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderItemAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Item de commande non trouvé" });
                }

                return Ok(new { success = true, message = "Item supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erreur lors de la suppression de l'item", details = ex.Message });
            }
        }

        /// <summary>
        /// Récupérer les commandes avec rendez-vous pour le calendrier
        /// </summary>
        [HttpGet("appointments")]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersWithAppointments(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var orders = await _orderService.GetOrdersWithAppointmentsAsync(startDate, endDate);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    // Classes de requête pour les endpoints spéciaux
    public class FinalTotalRequest
    {
        [Required]
        public decimal Total { get; set; }
        public decimal? Reduction { get; set; }
    }

    public class UpdateStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}