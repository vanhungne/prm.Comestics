using Microsoft.Extensions.Logging;
using Repository.Entities;
using Repository.Interface;
using Repository.Interfaces;
using Repository.Model.CheckOut;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(
            IOrderRepository orderRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            IPaymentRepository paymentRepository,
            ILogger<CheckoutService> logger)
        {
            _orderRepository = orderRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request, int userId)
        {
            try
            {
                // Validate stock availability
                var stockValid = await ValidateStockAvailabilityAsync(request.Items);
                if (!stockValid)
                {
                    return new CheckoutResponse
                    {
                        Success = false,
                        Message = "Insufficient stock for one or more items"
                    };
                }

                // Get products
                var productIds = request.Items.Select(i => i.ProductId).ToList();
                var products = await _productRepository.GetProductsByIdsAsync(productIds);

                // Calculate total amount
                decimal totalAmount = 0;
                var orderDetails = new List<OrderDetail>();

                foreach (var item in request.Items)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null)
                    {
                        return new CheckoutResponse
                        {
                            Success = false,
                            Message = $"Product with ID {item.ProductId} not found"
                        };
                    }

                    var itemTotal = product.Price * item.Quantity;
                    totalAmount += itemTotal;

                    orderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });
                }

                // Create order với status Pending cho PayPal
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    Status = request.PaymentMethod.ToUpper() == "PAYPAL" ? "Pending" : "Pending",
                    OrderDetails = orderDetails
                };

                var createdOrder = await _orderRepository.CreateOrderAsync(order);

                // Xử lý theo payment method
                if (request.PaymentMethod.ToUpper() == "PAYPAL")
                {
                    // Không tạo payment record ngay, chờ PayPal callback
                    // Không update stock ngay, chờ payment thành công
                    return new CheckoutResponse
                    {
                        Success = true,
                        Message = "Order created, awaiting PayPal payment",
                        OrderId = createdOrder.Id,
                        TotalAmount = totalAmount,
                        PaymentStatus = "Pending",
                        RequiresPaymentRedirect = true
                    };
                }
                else
                {
                    // Cash payment - process immediately
                    return await ProcessCashPaymentAsync(createdOrder, products, request.Items);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout process");
                return new CheckoutResponse
                {
                    Success = false,
                    Message = "An error occurred during checkout"
                };
            }
        }

        private async Task<CheckoutResponse> ProcessCashPaymentAsync(Order order, List<Product> products, List<CheckoutItem> items)
        {
            try
            {
                // Create payment record
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    PaymentMethod = "Cash",
                    Status = "Completed",
                    PaymentDate = DateTime.UtcNow
                };

                await _paymentRepository.CreatePaymentAsync(payment);

                // Update product stock
                foreach (var item in items)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                // Update order status
                order.Status = "Completed";
                await _orderRepository.UpdateOrderAsync(order);

                return new CheckoutResponse
                {
                    Success = true,
                    Message = "Checkout completed successfully",
                    OrderId = order.Id,
                    TotalAmount = order.TotalAmount,
                    PaymentStatus = "Completed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cash payment");
                throw;
            }
        }

        public async Task<CheckoutResponse> CompletePayPalPaymentAsync(int orderId, string paypalTransactionId)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return new CheckoutResponse
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                if (order.Status != "Pending")
                {
                    return new CheckoutResponse
                    {
                        Success = false,
                        Message = "Order is not in pending status"
                    };
                }

                // Create payment record
                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = order.TotalAmount,
                    PaymentMethod = "PayPal",
                    Status = "Completed",
                    PaymentDate = DateTime.UtcNow,
                    TransactionId = paypalTransactionId // Thêm field này vào Payment entity
                };

                await _paymentRepository.CreatePaymentAsync(payment);

                // Update product stock
                foreach (var orderDetail in order.OrderDetails)
                {
                    var product = await _productRepository.GetByIdAsync(orderDetail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= orderDetail.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                // Update order status
                order.Status = "Completed";
                await _orderRepository.UpdateOrderAsync(order);

                return new CheckoutResponse
                {
                    Success = true,
                    Message = "PayPal payment completed successfully",
                    OrderId = orderId,
                    TotalAmount = order.TotalAmount,
                    PaymentStatus = "Completed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing PayPal payment for order {OrderId}", orderId);
                return new CheckoutResponse
                {
                    Success = false,
                    Message = "An error occurred while completing payment"
                };
            }
        }

        public async Task<CheckoutResponse> CheckoutFromCartAsync(int userId, string paymentMethod = "Cash")
        {
            try
            {
                // Get cart items
                var cartItems = await _cartItemRepository.GetCartItemsByUserIdAsync(userId);

                if (!cartItems.Any())
                {
                    return new CheckoutResponse
                    {
                        Success = false,
                        Message = "Cart is empty"
                    };
                }

                // Convert cart items to checkout items
                var checkoutItems = cartItems.Select(ci => new CheckoutItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity
                }).ToList();

                var checkoutRequest = new CheckoutRequest
                {
                    PaymentMethod = paymentMethod,
                    Items = checkoutItems
                };

                // Process checkout
                var result = await CheckoutAsync(checkoutRequest, userId);

                // If checkout successful và không phải PayPal, clear cart
                if (result.Success && paymentMethod.ToUpper() != "PAYPAL")
                {
                    await _cartItemRepository.DeleteCartItemsAsync(cartItems);
                }
                // Nếu là PayPal, chờ payment complete mới clear cart

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cart checkout process");
                return new CheckoutResponse
                {
                    Success = false,
                    Message = "An error occurred during cart checkout"
                };
            }
        }

        public async Task<bool> ClearCartAfterPayPalPaymentAsync(int userId, int orderId)
        {
            try
            {
                var cartItems = await _cartItemRepository.GetCartItemsByUserIdAsync(userId);
                if (cartItems.Any())
                {
                    await _cartItemRepository.DeleteCartItemsAsync(cartItems);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart after PayPal payment");
                return false;
            }
        }

        public async Task<Order> GetOrderDetailsAsync(int orderId)
        {
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task<bool> ValidateStockAvailabilityAsync(List<CheckoutItem> items)
        {
            try
            {
                var productIds = items.Select(i => i.ProductId).ToList();
                var products = await _productRepository.GetProductsByIdsAsync(productIds);

                foreach (var item in items)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null || product.StockQuantity < item.Quantity)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating stock availability");
                return false;
            }
        }
    }
}
