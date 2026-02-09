using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using VideoRentalSystem.Models.Entities;

namespace VideoRentalSystem.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MediaType> MediaTypes { get; set; }
        public DbSet<MediaItem> MediaItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; } // Добавили

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурации отношений и индексов
            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new { w.CustomerId, w.MovieId })
                .IsUnique();

            // Конфигурация для ShoppingCartItem
            modelBuilder.Entity<ShoppingCartItem>()
                .HasIndex(s => s.SessionId);

            // Конфигурация для Order
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();
        }
    }
}