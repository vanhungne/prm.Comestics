using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Model.Cartt;
using Service.Interface;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class cartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public cartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: api/cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.GetCartAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // POST: api/cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _cartService.AddToCartAsync(userId, request.ProductId, request.Quantity);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // PUT: api/cart/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _cartService.UpdateCartItemAsync(userId, request.ProductId, request.Quantity);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // DELETE: api/cart/remove/{productId}
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.RemoveFromCartAsync(userId, productId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // DELETE: api/cart/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.ClearCartAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // GET: api/cart/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = GetCurrentUserId();
            var count = await _cartService.GetCartItemCountAsync(userId);

            return Ok(new { count = count });
        }

        // GET: api/cart/total
        [HttpGet("total")]
        public async Task<IActionResult> GetCartTotal()
        {
            var userId = GetCurrentUserId();
            var total = await _cartService.GetCartTotalAsync(userId);

            return Ok(new { total = total });
        }

        private int GetCurrentUserId()
        {
            // Get user ID from JWT token claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
    }
}
