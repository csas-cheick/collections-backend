using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> User { get; set; }
        public DbSet<Modele> Modele { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Measure> Measure { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>{entity.ToTable("user"); });
            modelBuilder.Entity<Modele>(entity =>{entity.ToTable("modele"); });
            modelBuilder.Entity<Transaction>(entity =>{entity.ToTable("transaction"); });
            modelBuilder.Entity<Customer>(entity =>{entity.ToTable("customer"); });
            modelBuilder.Entity<Measure>(entity =>{entity.ToTable("measure"); });
            modelBuilder.Entity<Order>(entity =>{entity.ToTable("order"); });
            modelBuilder.Entity<OrderItem>(entity =>{entity.ToTable("orderitem"); });

            // Configuration de la relation Customer -> Measure (One-to-One)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Measure)
                .WithOne(m => m.Customer)
                .HasForeignKey<Measure>(m => m.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration des index pour optimiser les performances
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.PhoneNumber)
                .IsUnique();

            // Configuration des précisions décimales pour les mesures
            modelBuilder.Entity<Measure>()
                .Property(m => m.TourPoitrine)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Measure>()
                .Property(m => m.TourCeinture)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Measure>()
                .Property(m => m.LongueurManche)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Measure>()
                .Property(m => m.TourBras)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Measure>()
                .Property(m => m.LongueurChemise)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Measure>()
                .Property(m => m.LongueurPantalon)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Measure>()
                .Property(m => m.LargeurEpaules)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Measure>()
                .Property(m => m.TourCou)
                .HasPrecision(5, 2);

            // Configuration des relations Order -> Customer (Many-to-One)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration des relations Order -> OrderItems (One-to-Many)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration des relations OrderItem -> Modele (Many-to-One)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Modele)
                .WithMany()
                .HasForeignKey(oi => oi.ModeleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration des précisions décimales pour les commandes
            modelBuilder.Entity<Order>()
                .Property(o => o.Total)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.Reduction)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalFinal)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.PrixUnitaire)
                .HasPrecision(8, 2);

            // Index pour optimiser les performances des commandes
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CustomerId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.DateCommande);

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId);
        }
    }
}