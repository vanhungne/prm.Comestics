using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Repository.Model.CheckOut;
using Service.Interface;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class checkoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;

        public checkoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        // POST: api/checkout
        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _checkoutService.CheckoutAsync(request, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // POST: api/checkout/cart
        [HttpPost("cart")]
        public async Task<IActionResult> CheckoutFromCart([FromBody] CheckoutFromCartRequest request)
        {
            var userId = GetCurrentUserId();
            var paymentMethod = request?.PaymentMethod ?? "Cash";

            var result = await _checkoutService.CheckoutFromCartAsync(userId, paymentMethod);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // GET: api/checkout/order/{orderId}
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _checkoutService.GetOrderDetailsAsync(orderId);
            if (order == null)
                return NotFound("Order not found");

            return Ok(order);
        }

        // POST: api/checkout/validate-stock
        [HttpPost("validate-stock")]
        public async Task<IActionResult> ValidateStock([FromBody] ValidateStockRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isValid = await _checkoutService.ValidateStockAvailabilityAsync(request.Items);
            return Ok(new { isValid = isValid });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
    }

    // Additional request models
    public class CheckoutFromCartRequest
    {
        public string PaymentMethod { get; set; } = "Cash";
    }

    public class ValidateStockRequest
    {
        public List<CheckoutItem> Items { get; set; } = new List<CheckoutItem>();
    }
}
