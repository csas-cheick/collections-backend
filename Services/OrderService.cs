using backend.Data;
using backend.Dtos;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync()
        {
            return await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.Name,
                    DateCommande = o.DateCommande,
                    DateRendezVous = o.DateRendezVous,
                    Total = o.Total,
                    Reduction = o.Reduction,
                    TotalFinal = o.TotalFinal,
                    Statut = o.Statut,
                    NombreItems = o.OrderItems.Count,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
        {
            var order = await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Modele)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return null;

            return new OrderResponseDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.Name,
                DateCommande = order.DateCommande,
                DateRendezVous = order.DateRendezVous,
                Total = order.Total,
                Reduction = order.Reduction,
                TotalFinal = order.TotalFinal,
                Statut = order.Statut,
                Notes = order.Notes,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ModeleId = oi.ModeleId,
                    TypeTissu = oi.TypeTissu,
                    Couleur = oi.Couleur,
                    PrixUnitaire = oi.PrixUnitaire,
                    Quantite = oi.Quantite,
                    Notes = oi.Notes
                }).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }

        public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto createDto)
        {
            // Vérifier que le client existe
            var customer = await _context.Customer.FindAsync(createDto.CustomerId);
            if (customer == null)
                throw new ArgumentException("Client non trouvé");

            // Calculer le total de la commande
            var total = await CalculateOrderTotalAsync(createDto.OrderItems);
            var finalTotal = await CalculateFinalTotalAsync(total, createDto.Reduction);

            // Créer la commande
            var order = new Order
            {
                CustomerId = createDto.CustomerId,
                DateCommande = createDto.DateCommande,
                DateRendezVous = createDto.DateRendezVous,
                Total = total,
                Reduction = createDto.Reduction,
                TotalFinal = finalTotal,
                Statut = createDto.Statut,
                Notes = createDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            // Créer les items de la commande
            foreach (var itemDto in createDto.OrderItems)
            {
                var modele = await _context.Modele.FindAsync(itemDto.ModeleId);
                if (modele == null)
                    throw new ArgumentException($"Modèle {itemDto.ModeleId} non trouvé");

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ModeleId = itemDto.ModeleId,
                    TypeTissu = itemDto.TypeTissu,
                    Couleur = itemDto.Couleur,
                    PrixUnitaire = modele.Price,
                    Quantite = itemDto.Quantite,
                    Notes = itemDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.OrderItem.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            // Retourner la commande créée
            return await GetOrderByIdAsync(order.Id) ?? throw new InvalidOperationException("Erreur lors de la création de la commande");
        }

        public async Task<OrderResponseDto?> UpdateOrderAsync(int id, UpdateOrderDto updateDto)
        {
            var order = await _context.Order
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return null;

            // Mettre à jour les propriétés de la commande
            if (updateDto.CustomerId.HasValue)
            {
                var customer = await _context.Customer.FindAsync(updateDto.CustomerId.Value);
                if (customer == null)
                    throw new ArgumentException("Client non trouvé");
                order.CustomerId = updateDto.CustomerId.Value;
            }

            if (updateDto.DateCommande.HasValue)
                order.DateCommande = updateDto.DateCommande.Value;

            if (updateDto.DateRendezVous.HasValue)
                order.DateRendezVous = updateDto.DateRendezVous.Value;

            if (!string.IsNullOrEmpty(updateDto.Statut))
                order.Statut = updateDto.Statut;

            if (updateDto.Notes != null)
                order.Notes = updateDto.Notes;

            // Mettre à jour les items si fournis
            if (updateDto.OrderItems != null)
            {
                // Supprimer les anciens items
                _context.OrderItem.RemoveRange(order.OrderItems);

                // Ajouter les nouveaux items
                foreach (var itemDto in updateDto.OrderItems)
                {
                    var modele = await _context.Modele.FindAsync(itemDto.ModeleId);
                    if (modele == null)
                        throw new ArgumentException($"Modèle {itemDto.ModeleId} non trouvé");

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ModeleId = itemDto.ModeleId,
                        TypeTissu = itemDto.TypeTissu,
                        Couleur = itemDto.Couleur,
                        PrixUnitaire = modele.Price,
                        Quantite = itemDto.Quantite,
                        Notes = itemDto.Notes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.OrderItem.Add(orderItem);
                }

                // Recalculer le total
                order.Total = await CalculateOrderTotalAsync(updateDto.OrderItems);
            }

            // Mettre à jour la réduction et le total final
            if (updateDto.Reduction.HasValue)
                order.Reduction = updateDto.Reduction.Value;

            order.TotalFinal = await CalculateFinalTotalAsync(order.Total, order.Reduction);
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(order.Id);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null)
                return false;

            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateOrderTotalAsync(List<CreateOrderItemDto> items)
        {
            decimal total = 0;

            foreach (var item in items)
            {
                var modele = await _context.Modele.FindAsync(item.ModeleId);
                if (modele != null)
                {
                    total += modele.Price * item.Quantite;
                }
            }

            return total;
        }

        public async Task<decimal> CalculateFinalTotalAsync(decimal total, decimal? reduction)
        {
            await Task.CompletedTask; // Pour satisfaire l'interface async
            return reduction.HasValue ? Math.Max(0, total - reduction.Value) : total;
        }

        public async Task<OrderItemDto?> GetOrderItemByIdAsync(int id)
        {
            var orderItem = await _context.OrderItem
                .Include(oi => oi.Modele)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null)
                return null;

            return new OrderItemDto
            {
                Id = orderItem.Id,
                ModeleId = orderItem.ModeleId,
                TypeTissu = orderItem.TypeTissu,
                Couleur = orderItem.Couleur,
                PrixUnitaire = orderItem.PrixUnitaire,
                Quantite = orderItem.Quantite,
                Notes = orderItem.Notes
            };
        }

        public async Task<OrderItemDto> AddOrderItemAsync(int orderId, CreateOrderItemDto itemDto)
        {
            var order = await _context.Order.FindAsync(orderId);
            if (order == null)
                throw new ArgumentException("Commande non trouvée");

            var modele = await _context.Modele.FindAsync(itemDto.ModeleId);
            if (modele == null)
                throw new ArgumentException("Modèle non trouvé");

            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ModeleId = itemDto.ModeleId,
                TypeTissu = itemDto.TypeTissu,
                Couleur = itemDto.Couleur,
                PrixUnitaire = modele.Price,
                Quantite = itemDto.Quantite,
                Notes = itemDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.OrderItem.Add(orderItem);

            // Recalculer le total de la commande
            var orderItems = await _context.OrderItem
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            order.Total = orderItems.Sum(oi => oi.PrixUnitaire * oi.Quantite) + (modele.Price * itemDto.Quantite);
            order.TotalFinal = await CalculateFinalTotalAsync(order.Total, order.Reduction);
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new OrderItemDto
            {
                Id = orderItem.Id,
                ModeleId = orderItem.ModeleId,
                TypeTissu = orderItem.TypeTissu,
                Couleur = orderItem.Couleur,
                PrixUnitaire = orderItem.PrixUnitaire,
                Quantite = orderItem.Quantite,
                Notes = orderItem.Notes
            };
        }

        public async Task<OrderItemDto?> UpdateOrderItemAsync(int id, UpdateOrderItemDto itemDto)
        {
            var orderItem = await _context.OrderItem
                .Include(oi => oi.Order)
                .Include(oi => oi.Modele)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null)
                return null;

            // Mettre à jour les propriétés
            if (itemDto.ModeleId.HasValue)
            {
                var modele = await _context.Modele.FindAsync(itemDto.ModeleId.Value);
                if (modele == null)
                    throw new ArgumentException("Modèle non trouvé");

                orderItem.ModeleId = itemDto.ModeleId.Value;
                orderItem.PrixUnitaire = modele.Price;
            }

            if (!string.IsNullOrEmpty(itemDto.TypeTissu))
                orderItem.TypeTissu = itemDto.TypeTissu;

            if (!string.IsNullOrEmpty(itemDto.Couleur))
                orderItem.Couleur = itemDto.Couleur;

            if (itemDto.Quantite.HasValue)
                orderItem.Quantite = itemDto.Quantite.Value;

            if (itemDto.Notes != null)
                orderItem.Notes = itemDto.Notes;

            orderItem.UpdatedAt = DateTime.UtcNow;

            // Recalculer le total de la commande
            var order = orderItem.Order;
            var allOrderItems = await _context.OrderItem
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            order.Total = allOrderItems.Sum(oi => oi.PrixUnitaire * oi.Quantite);
            order.TotalFinal = await CalculateFinalTotalAsync(order.Total, order.Reduction);
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetOrderItemByIdAsync(orderItem.Id);
        }

        public async Task<bool> DeleteOrderItemAsync(int id)
        {
            var orderItem = await _context.OrderItem
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null)
                return false;

            var order = orderItem.Order;
            _context.OrderItem.Remove(orderItem);

            // Recalculer le total de la commande
            var remainingItems = await _context.OrderItem
                .Where(oi => oi.OrderId == order.Id && oi.Id != id)
                .ToListAsync();

            order.Total = remainingItems.Sum(oi => oi.PrixUnitaire * oi.Quantite);
            order.TotalFinal = await CalculateFinalTotalAsync(order.Total, order.Reduction);
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.Name,
                    DateCommande = o.DateCommande,
                    Total = o.Total,
                    Reduction = o.Reduction,
                    TotalFinal = o.TotalFinal,
                    Statut = o.Statut,
                    NombreItems = o.OrderItems.Count,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(string status)
        {
            return await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.Statut == status)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.Name,
                    DateCommande = o.DateCommande,
                    Total = o.Total,
                    Reduction = o.Reduction,
                    TotalFinal = o.TotalFinal,
                    Statut = o.Statut,
                    NombreItems = o.OrderItems.Count,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null)
                return false;

            order.Statut = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetOrdersWithAppointmentsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Order
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.DateRendezVous != null); // Seulement les commandes avec RDV

            // Filtrer par période si spécifiée
            if (startDate.HasValue)
            {
                query = query.Where(o => o.DateRendezVous >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.DateRendezVous <= endDate.Value);
            }

            return await query
                .OrderBy(o => o.DateRendezVous) // Trier par date de RDV
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.Name,
                    DateCommande = o.DateCommande,
                    DateRendezVous = o.DateRendezVous,
                    Total = o.Total,
                    Reduction = o.Reduction,
                    TotalFinal = o.TotalFinal,
                    Statut = o.Statut,
                    NombreItems = o.OrderItems.Count,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }
    }
}