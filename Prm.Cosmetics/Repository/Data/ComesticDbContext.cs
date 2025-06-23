using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data
{
    public class ComesticDbContext : DbContext
    {
        public ComesticDbContext()
        {
        }
        public ComesticDbContext(DbContextOptions<ComesticDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // User - Role (1 Role : Many Users)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Orders (1 User : Many Orders)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - CartItems (1 User : Many CartItems)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product - CartItems (1 Product : Many CartItems)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order - OrderDetails (1 Order : Many OrderDetails)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product - OrderDetails (1 Product : Many OrderDetails)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order - Payment (1 Order : 1 Payment)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed roles (optional)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Customer" }
            );

            modelBuilder.Entity<User>().HasData(
    new User
    {
        Id = 1,
        FullName = "Admin User",
        Email = "admin@example.com",
        PasswordHash = "$2a$11$Pn4clTN60Zfkw7B2n8Z4Pewe89PhVHDWH1i31Qr.K1OZp2YRHNOmO",
        RoleId = 1,
        CreatedAt = DateTime.Now
    },
    new User
    {
        Id = 2,
        FullName = "Customer One",
        Email = "customer1@example.com",
        PasswordHash = "$2a$11$Pn4clTN60Zfkw7B2n8Z4Pewe89PhVHDWH1i31Qr.K1OZp2YRHNOmO",
        RoleId = 2,
        CreatedAt = DateTime.Now
    },
    new User
    {
        Id = 3,
        FullName = "Customer Two",
        Email = "customer2@example.com",
        PasswordHash = "$2a$11$Pn4clTN60Zfkw7B2n8Z4Pewe89PhVHDWH1i31Qr.K1OZp2YRHNOmO",
        RoleId = 2,
        CreatedAt = DateTime.Now
    }
);
        }

    }
}
