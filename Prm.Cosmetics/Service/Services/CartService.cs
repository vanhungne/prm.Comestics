using Microsoft.Extensions.Logging;
using Repository.Entities;
using Repository.Interface;
using Repository.Interfaces;
using Repository.Model.Cartt;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class CartService : ICartService
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            ILogger<CartService> logger)
        {
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<CartResponse> AddToCartAsync(int userId, int productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return new CartResponse
                    {
                        Success = false,
                        Message = "Quantity must be greater than 0"
                    };
                }

                // Check if product exists
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return new CartResponse
                    {
                        Success = false,
                        Message = "Product not found"
                    };
                }

                // Check stock availability
                if (product.StockQuantity < quantity)
                {
                    return new CartResponse
                    {
                        Success = false,
                        Message = $"Insufficient stock. Available: {product.StockQuantity}"
                    };
                }

                // Check if item already exists in cart
                var existingCartItem = await _cartItemRepository.GetCartItemAsync(userId, productId);

                if (existingCartItem != null)
                {
                    // Update quantity
                    var newQuantity = existingCartItem.Quantity + quantity;

                    if (product.StockQuantity < newQuantity)
                    {
                        return new CartResponse
                        {
                            Success = false,
                            Message = $"Cannot add {quantity} items. Total would exceed available stock ({product.StockQuantity})"
                        };
                    }

                    existingCartItem.Quantity = newQuantity;
                    await _cartItemRepository.UpdateCartItemAsync(existingCartItem);
                }
                else
                {
                    // Add new cart item
                    var cartItem = new CartItem
                    {
                        UserId = userId,
                        ProductId = productId,
                        Quantity = quantity,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _cartItemRepository.AddCartItemAsync(cartItem);
                }

                return new CartResponse
                {
                    Success = true,
                    Message = "Item added to cart successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return new CartResponse
                {
                    Success = false,
                    Message = "An error occurred while adding item to cart"
                };
            }
        }

        public async Task<CartResponse> UpdateCartItemAsync(int userId, int productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return await RemoveFromCartAsync(userId, productId);
                }

                var cartItem = await _cartItemRepository.GetCartItemAsync(userId, productId);
                if (cartItem == null)
                {
                    return new CartResponse
                    {
                        Success = false,
                        Message = "Cart item not found"
                    };
                }

                // Check stock availability
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return new CartResponse
                    {
                        Success = false,
                        Message = "Product not found"
                    };
                }

                if (product.StockQuantity < quantity)
                {
                    return new CartResponse
                    {
                        Success = false,
                        Message = $"Insufficient stock. Available: {product.StockQuantity}"
                    };
                }

                cartItem.Quantity = quantity;
                await _cartItemRepository.UpdateCartItemAsync(cartItem);

                return new CartResponse
                {
                    Success = true,
                    Message = "Cart item updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return new CartResponse
                {
                    Success = false,
                    Message = "An error occurred while updating cart item"
                };
            }
        }

        public async Task<CartResponse> RemoveFromCartAsync(int userId, int productId)
        {
            try
            {
                var cartItem = await _cartItemRepository.GetCartItemAsync(userId, productId);
                if (cartItem == null)
                {
                    return new CartResponse
                    {
                        Success = false,
                        Message = "Cart item not found"
                    };
                }

                await _cartItemRepository.DeleteCartItemAsync(cartItem);

                return new CartResponse
                {
                    Success = true,
                    Message = "Item removed from cart successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return new CartResponse
                {
                    Success = false,
                    Message = "An error occurred while removing item from cart"
                };
            }
        }

        public async Task<CartResponse> GetCartAsync(int userId)
        {
            try
            {
                var cartItems = await _cartItemRepository.GetCartItemsByUserIdAsync(userId);

                var cartItemDtos = cartItems.Select(ci => new CartItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Name,
                    ProductPrice = ci.Product?.Price ?? 0,
                    Quantity = ci.Quantity,
                    Total = (ci.Product?.Price ?? 0) * ci.Quantity,
                    StockQuantity = ci.Product?.StockQuantity ?? 0
                }).ToList();

                var totalAmount = cartItemDtos.Sum(item => item.Total);
                var totalItems = cartItemDtos.Sum(item => item.Quantity);

                return new CartResponse
                {
                    Success = true,
                    Message = "Cart retrieved successfully",
                    Items = cartItemDtos,
                    TotalAmount = totalAmount,
                    TotalItems = totalItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart");
                return new CartResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving cart"
                };
            }
        }

        public async Task<CartResponse> ClearCartAsync(int userId)
        {
            try
            {
                var cartItems = await _cartItemRepository.GetCartItemsByUserIdAsync(userId);

                if (cartItems.Any())
                {
                    await _cartItemRepository.DeleteCartItemsAsync(cartItems);
                }

                return new CartResponse
                {
                    Success = true,
                    Message = "Cart cleared successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return new CartResponse
                {
                    Success = false,
                    Message = "An error occurred while clearing cart"
                };
            }
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            try
            {
                var cartItems = await _cartItemRepository.GetCartItemsByUserIdAsync(userId);
                return cartItems.Sum(ci => ci.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count");
                return 0;
            }
        }

        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            try
            {
                var cartItems = await _cartItemRepository.GetCartItemsByUserIdAsync(userId);
                return cartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total");
                return 0;
            }
        }
    }
}