using ProductService.Models;

namespace ProductService.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Products.Any())
        {
            var products = new Product[]
            {
                new Product { Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Quantity = 50 },
                new Product { Name = "Smartphone", Description = "Latest smartphone model", Price = 699.99m, Quantity = 100 },
                new Product { Name = "Headphones", Description = "Wireless noise-canceling headphones", Price = 199.99m, Quantity = 200 },
                new Product { Name = "Tablet", Description = "10-inch tablet with stylus", Price = 449.99m, Quantity = 75 },
                new Product { Name = "Smartwatch", Description = "Fitness tracking smartwatch", Price = 299.99m, Quantity = 150 }
            };

            context.Products.AddRange(products);
        }

        if (!context.Users.Any())
        {
            var users = new User[]
            {
                new User { Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Email = "admin@example.com", Role = "Admin" },
                new User { Username = "user", PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"), Email = "user@example.com", Role = "User" }
            };

            context.Users.AddRange(users);
        }

        context.SaveChanges();
    }

    public static List<Product> GetDefaultProducts()
    {
        return new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Quantity = 50 },
            new Product { Id = 2, Name = "Smartphone", Description = "Latest smartphone model", Price = 699.99m, Quantity = 100 },
            new Product { Id = 3, Name = "Headphones", Description = "Wireless noise-canceling headphones", Price = 199.99m, Quantity = 200 },
            new Product { Id = 4, Name = "Tablet", Description = "10-inch tablet with stylus", Price = 449.99m, Quantity = 75 },
            new Product { Id = 5, Name = "Smartwatch", Description = "Fitness tracking smartwatch", Price = 299.99m, Quantity = 150 }
        };
    }
}
