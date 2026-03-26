using ProductService.Models;

namespace ProductService.Services;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<Order?> GetByIdAsync(int id);
    Task<Order> CreateAsync(int userId, CreateOrderRequest request);
    Task<Order?> UpdateStatusAsync(int id, string status);
    Task<bool> CancelAsync(int id);
}
