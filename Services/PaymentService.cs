using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly Random _random = new();

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentResponse> ProcessUpiPaymentAsync(UpiPaymentRequest request)
    {
        var order = await _context.Orders.FindAsync(request.OrderId);
        if (order == null)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = "Order not found"
            };
        }

        if (order.Status != "Pending")
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = "Order is not in pending status"
            };
        }

        // Validate UPI ID format
        if (!IsValidUpiId(request.UpiId))
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = "Invalid UPI ID format. Expected format: username@bankname"
            };
        }

        // Check for existing payment
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId && p.Status == "Completed");
        if (existingPayment != null)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = "Payment already completed for this order"
            };
        }

        // Simulate payment processing
        var payment = new Payment
        {
            OrderId = request.OrderId,
            PaymentMethod = "UPI",
            Amount = order.TotalAmount,
            TransactionId = GenerateTransactionId("UPI"),
            Status = "Processing",
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Simulate payment gateway response (90% success rate for demo)
        var isSuccess = SimulatePaymentGateway();

        if (isSuccess)
        {
            payment.Status = "Completed";
            payment.CompletedAt = DateTime.UtcNow;
            order.Status = "Processing";

            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                PaymentId = payment.Id,
                TransactionId = payment.TransactionId,
                Status = "Completed",
                Message = "Payment successful via UPI"
            };
        }
        else
        {
            payment.Status = "Failed";
            payment.FailureReason = "Payment declined by bank";

            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                PaymentId = payment.Id,
                TransactionId = payment.TransactionId,
                Status = "Failed",
                Message = "Payment failed. Please try again."
            };
        }
    }

    public async Task<PaymentResponse> ProcessDebitCardPaymentAsync(DebitCardPaymentRequest request)
    {
        var order = await _context.Orders.FindAsync(request.OrderId);
        if (order == null)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = "Order not found"
            };
        }

        if (order.Status != "Pending")
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = "Order is not in pending status"
            };
        }

        // Validate card details
        var validationResult = ValidateDebitCard(request);
        if (!validationResult.IsValid)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = validationResult.Message
            };
        }

        // Check for existing payment
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId && p.Status == "Completed");
        if (existingPayment != null)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = "Payment already completed for this order"
            };
        }

        // Simulate payment processing
        var payment = new Payment
        {
            OrderId = request.OrderId,
            PaymentMethod = "DebitCard",
            Amount = order.TotalAmount,
            TransactionId = GenerateTransactionId("DC"),
            Status = "Processing",
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Simulate payment scenarios
        var scenario = SimulatePaymentScenario();

        switch (scenario)
        {
            case PaymentScenario.Success:
                payment.Status = "Completed";
                payment.CompletedAt = DateTime.UtcNow;
                order.Status = "Processing";
                await _context.SaveChangesAsync();

                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    TransactionId = payment.TransactionId,
                    Status = "Completed",
                    Message = "Payment successful via Debit Card"
                };

            case PaymentScenario.InsufficientFunds:
                payment.Status = "Failed";
                payment.FailureReason = "Insufficient funds";
                await _context.SaveChangesAsync();

                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    TransactionId = payment.TransactionId,
                    Status = "Failed",
                    Message = "Payment failed: Insufficient funds in your account"
                };

            case PaymentScenario.CardExpired:
                payment.Status = "Failed";
                payment.FailureReason = "Card expired";
                await _context.SaveChangesAsync();

                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    TransactionId = payment.TransactionId,
                    Status = "Failed",
                    Message = "Payment failed: Card has expired"
                };

            case PaymentScenario.NetworkError:
                payment.Status = "Failed";
                payment.FailureReason = "Network timeout";
                await _context.SaveChangesAsync();

                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    TransactionId = payment.TransactionId,
                    Status = "Failed",
                    Message = "Payment failed: Network error. Please try again."
                };

            case PaymentScenario.FraudDetected:
                payment.Status = "Failed";
                payment.FailureReason = "Suspicious activity detected";
                await _context.SaveChangesAsync();

                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    TransactionId = payment.TransactionId,
                    Status = "Failed",
                    Message = "Payment blocked: Suspicious activity detected. Please contact your bank."
                };

            default:
                payment.Status = "Failed";
                payment.FailureReason = "Unknown error";
                await _context.SaveChangesAsync();

                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    TransactionId = payment.TransactionId,
                    Status = "Failed",
                    Message = "Payment failed: Unknown error occurred"
                };
        }
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .Where(p => p.Order.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PaymentResponse> RefundPaymentAsync(int paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = "Payment not found"
            };
        }

        if (payment.Status != "Completed")
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = "Only completed payments can be refunded"
            };
        }

        // Simulate refund processing (95% success rate)
        var isSuccess = _random.Next(100) < 95;

        if (isSuccess)
        {
            payment.Status = "Refunded";
            payment.Order.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                PaymentId = payment.Id,
                TransactionId = payment.TransactionId,
                Status = "Refunded",
                Message = "Refund processed successfully. Amount will be credited within 5-7 business days."
            };
        }

        return new PaymentResponse
        {
            PaymentId = payment.Id,
            TransactionId = payment.TransactionId,
            Status = "Failed",
            Message = "Refund failed. Please try again later."
        };
    }

    public async Task<PaymentResponse> CheckPaymentStatusAsync(int paymentId)
    {
        var payment = await _context.Payments.FindAsync(paymentId);

        if (payment == null)
        {
            return new PaymentResponse
            {
                Status = "Failed",
                Message = "Payment not found"
            };
        }

        return new PaymentResponse
        {
            PaymentId = payment.Id,
            TransactionId = payment.TransactionId,
            Status = payment.Status,
            Message = payment.Status == "Completed" 
                ? "Payment completed successfully" 
                : payment.FailureReason ?? "Payment is " + payment.Status.ToLower()
        };
    }

    private bool IsValidUpiId(string upiId)
    {
        if (string.IsNullOrWhiteSpace(upiId)) return false;
        var parts = upiId.Split('@');
        return parts.Length == 2 && parts[0].Length > 0 && parts[1].Length > 0;
    }

    private (bool IsValid, string Message) ValidateDebitCard(DebitCardPaymentRequest request)
    {
        // Card number validation (basic Luhn check simulation)
        if (string.IsNullOrWhiteSpace(request.CardNumber) || request.CardNumber.Length < 13 || request.CardNumber.Length > 19)
        {
            return (false, "Invalid card number");
        }

        if (!request.CardNumber.All(char.IsDigit))
        {
            return (false, "Card number must contain only digits");
        }

        // Expiry validation
        if (!int.TryParse(request.ExpiryMonth, out var month) || month < 1 || month > 12)
        {
            return (false, "Invalid expiry month");
        }

        if (!int.TryParse(request.ExpiryYear, out var year))
        {
            return (false, "Invalid expiry year");
        }

        var currentYear = DateTime.Now.Year % 100;
        var currentMonth = DateTime.Now.Month;

        if (year < currentYear || (year == currentYear && month < currentMonth))
        {
            return (false, "Card has expired");
        }

        // CVV validation
        if (string.IsNullOrWhiteSpace(request.Cvv) || request.Cvv.Length < 3 || request.Cvv.Length > 4)
        {
            return (false, "Invalid CVV");
        }

        // Cardholder name validation
        if (string.IsNullOrWhiteSpace(request.CardHolderName))
        {
            return (false, "Cardholder name is required");
        }

        return (true, "Valid");
    }

    private string GenerateTransactionId(string prefix)
    {
        return $"{prefix}{DateTime.UtcNow:yyyyMMddHHmmss}{_random.Next(10000, 99999)}";
    }

    private bool SimulatePaymentGateway()
    {
        // 90% success rate for demo
        return _random.Next(100) < 90;
    }

    private PaymentScenario SimulatePaymentScenario()
    {
        var chance = _random.Next(100);
        
        // 80% success, 20% various failures
        if (chance < 80) return PaymentScenario.Success;
        if (chance < 85) return PaymentScenario.InsufficientFunds;
        if (chance < 90) return PaymentScenario.CardExpired;
        if (chance < 95) return PaymentScenario.NetworkError;
        return PaymentScenario.FraudDetected;
    }

    private enum PaymentScenario
    {
        Success,
        InsufficientFunds,
        CardExpired,
        NetworkError,
        FraudDetected
    }
}
