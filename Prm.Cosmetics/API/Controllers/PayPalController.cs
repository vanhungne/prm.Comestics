using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Service.Interface;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class PayPalController : ControllerBase
{
    private readonly IPaypalService _payPalService;
    private readonly IOrderService _orderService;
    private readonly ICheckoutService _checkoutService;
    private readonly ILogger<PayPalController> _logger;

    public PayPalController(
        IPaypalService payPalService,
        IOrderService orderService,
        ICheckoutService checkoutService,
        ILogger<PayPalController> logger)
    {
        _payPalService = payPalService;
        _orderService = orderService;
        _checkoutService = checkoutService;
        _logger = logger;
    }

    [HttpPost("create-payment")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        if (request.OrderId <= 0)
        {
            return BadRequest("Invalid order ID.");
        }

        try
        {
            var order = await _orderService.GetOrderByIdAsync(request.OrderId);
            if (order == null)
            {
                return NotFound($"Order {request.OrderId} not found");
            }

            if (order.Status != "Pending")
            {
                return BadRequest("Order is not in pending status");
            }

            var approvalUrl = await _payPalService.CreatePaymentAsync(order.TotalAmount, request.OrderId);

            return Ok(new
            {
                success = true,
                approvalUrl = approvalUrl,
                orderId = request.OrderId,
                amount = order.TotalAmount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayPal payment for order {OrderId}", request.OrderId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpGet("capture-payment")]
    public async Task<IActionResult> CapturePayment([FromQuery] string token, [FromQuery] string orderNumber)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(orderNumber))
        {
            return BadRequest("Token and orderNumber are required.");
        }

        try
        {
            // Capture payment từ PayPal
            var captureResult = await _payPalService.CapturePaymentAsync(token);

            if (captureResult.Status == "COMPLETED")
            {
                // Complete order trong hệ thống
                if (int.TryParse(orderNumber, out int orderId))
                {
                    var completeResult = await _checkoutService.CompletePayPalPaymentAsync(orderId, captureResult.OrderId);

                    if (completeResult.Success)
                    {
                        // Clear cart nếu cần
                        var userId = GetCurrentUserId();
                        if (userId > 0)
                        {
                            await _checkoutService.ClearCartAfterPayPalPaymentAsync(userId, orderId);
                        }

                        return Ok(new
                        {
                            status = "Success",
                            message = "Payment completed successfully",
                            orderId = captureResult.OrderId,
                            orderNumber = orderNumber,
                            amount = completeResult.TotalAmount
                        });
                    }
                    else
                    {
                        return StatusCode(500, completeResult.Message);
                    }
                }
            }
            else
            {
                return Ok(new
                {
                    status = "Pending",
                    message = "Payment is being processed",
                    orderId = captureResult.OrderId
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing PayPal payment for token {Token}", token);
            return StatusCode(500, "An error occurred while processing the payment");
        }

        return StatusCode(500, "Payment processing failed");
    }

    [HttpGet("cancel-payment")]
    public IActionResult CancelPayment([FromQuery] string orderNumber)
    {
        _logger.LogInformation("PayPal payment cancelled for order {OrderNumber}", orderNumber);

        // Có thể redirect về trang cancel hoặc trả về response
        return Ok(new
        {
            status = "Cancelled",
            message = "Payment was cancelled by user",
            orderNumber = orderNumber
        });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        return 0; // Return 0 instead of throwing for this case
    }
}

// Request models
public class CreatePaymentRequest
{
    public int OrderId { get; set; }
}