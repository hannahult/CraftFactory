using IT.CraftOrders.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders.Data
{
    public class CraftFactoryDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public CraftFactoryDbContext(DbContextOptions<CraftFactoryDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            }
        }
        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Product>().HasIndex(x => x.Sku).IsUnique();
            b.Entity<Customer>().HasIndex(x => x.Email).IsUnique();

            b.Entity<OrderLine>()
                .HasOne(ol => ol.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(ol => ol.OrderId);

            b.Entity<OrderLine>()
                .HasOne(ol => ol.Product)
                .WithMany(p => p.OrderLines)
                .HasForeignKey(ol => ol.ProductId);

            b.Entity<Product>().HasData(
                new Product { ProductId = 1, Sku = "GLUE-STICK", Name = "Craft Glue Stick", Price = 19.90m, IsActive = true },
                new Product { ProductId = 2, Sku = "GLITTER-KIT", Name = "Glitter Kit 6-pack", Price = 49.00m, IsActive = true },
                new Product { ProductId = 3, Sku = "PAPER-A4-COLOR", Name = "Coloured A4 Paper Pack", Price = 39.00m, IsActive = true },
                new Product { ProductId = 4, Sku = "BEADS-MIX", Name = "Mixed Beads 200g", Price = 59.00m, IsActive = true },
                new Product { ProductId = 5, Sku = "PAINT-SET", Name = "Acrylic Paint Set 12pcs", Price = 129.00m, IsActive = true },
                new Product { ProductId = 6, Sku = "BRUSH-SET", Name = "Fine Brush Set 6pcs", Price = 79.00m, IsActive = true },
                new Product { ProductId = 7, Sku = "SCISSORS-KIDS", Name = "Safety Scissors for Kids", Price = 29.90m, IsActive = true }
            );

            b.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, Email = "hanna@example.com", Name = "Hanna Hult", Phone = "0701234567", Address = "Examplegatan 1" },
                new Customer { CustomerId = 2, Email = "kevin@example.com", Name = "Kevin Selin", Phone = "0702345678", Address = "Examplegatan 2" }
            );
        } 
    }
}
