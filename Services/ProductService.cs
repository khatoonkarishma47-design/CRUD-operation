using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Services;

public class ProductServiceImpl : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly bool _databaseAvailable;

    public ProductServiceImpl(ApplicationDbContext context)
    {
        _context = context;
        _databaseAvailable = CheckDatabaseAvailability();
    }

    private bool CheckDatabaseAvailability()
    {
        try
        {
            return _context.Database.CanConnect();
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        if (!_databaseAvailable)
        {
            return DbInitializer.GetDefaultProducts();
        }
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        if (!_databaseAvailable)
        {
            return DbInitializer.GetDefaultProducts().FirstOrDefault(p => p.Id == id);
        }
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        if (!_databaseAvailable)
        {
            throw new InvalidOperationException("Database is not available. Cannot create product.");
        }
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(int id, Product product)
    {
        if (!_databaseAvailable)
        {
            throw new InvalidOperationException("Database is not available. Cannot update product.");
        }

        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            return null;
        }

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.Quantity = product.Quantity;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingProduct;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!_databaseAvailable)
        {
            throw new InvalidOperationException("Database is not available. Cannot delete product.");
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
