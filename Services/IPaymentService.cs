using ProductService.Models;

namespace ProductService.Services;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessUpiPaymentAsync(UpiPaymentRequest request);
    Task<PaymentResponse> ProcessDebitCardPaymentAsync(DebitCardPaymentRequest request);
    Task<Payment?> GetPaymentByIdAsync(int id);
    Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
    Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId);
    Task<PaymentResponse> RefundPaymentAsync(int paymentId);
    Task<PaymentResponse> CheckPaymentStatusAsync(int paymentId);
}
