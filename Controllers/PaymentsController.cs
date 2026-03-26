using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;

    public PaymentsController(IPaymentService paymentService, IOrderService orderService)
    {
        _paymentService = paymentService;
        _orderService = orderService;
    }

    [HttpPost("upi")]
    public async Task<ActionResult<PaymentResponse>> ProcessUpiPayment([FromBody] UpiPaymentRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Verify order belongs to user
        var order = await _orderService.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            return NotFound("Order not found");
        }

        if (order.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _paymentService.ProcessUpiPaymentAsync(request);
        
        if (result.Status == "Failed")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("debit-card")]
    public async Task<ActionResult<PaymentResponse>> ProcessDebitCardPayment([FromBody] DebitCardPaymentRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Verify order belongs to user
        var order = await _orderService.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            return NotFound("Order not found");
        }

        if (order.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _paymentService.ProcessDebitCardPaymentAsync(request);
        
        if (result.Status == "Failed")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Payment>> GetPayment(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (payment.Order.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return Ok(payment);
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<Payment>> GetPaymentByOrder(int orderId)
    {
        var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        if (payment == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (payment.Order.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return Ok(payment);
    }

    [HttpGet("my-payments")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetMyPayments()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var payments = await _paymentService.GetPaymentsByUserIdAsync(userId.Value);
        return Ok(payments);
    }

    [HttpPost("{id}/refund")]
    public async Task<ActionResult<PaymentResponse>> RefundPayment(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (payment.Order.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _paymentService.RefundPaymentAsync(id);
        
        if (result.Status == "Failed")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<PaymentResponse>> CheckStatus(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (payment.Order.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _paymentService.CheckPaymentStatusAsync(id);
        return Ok(result);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
