using Microsoft.Extensions.Logging;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentException("Invalid order ID", nameof(orderId));
            }
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task<Order> GetOrderByNumberAsync(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                throw new ArgumentException("Order number cannot be null or empty", nameof(orderNumber));
            }

            if (int.TryParse(orderNumber, out int orderId))
            {
                return await GetOrderByIdAsync(orderId);
            }

            throw new ArgumentException("Invalid order number format", nameof(orderNumber));
        }

        public async Task<bool> ProcessPaymentAsync(string orderNumber, string paypalTransactionId)
        {
            try
            {
                if (int.TryParse(orderNumber, out int orderId))
                {
                    var order = await _orderRepository.GetOrderByIdAsync(orderId);
                    if (order != null && order.Status == "Pending")
                    {
                        order.Status = "PaymentProcessing"; 
                        await _orderRepository.UpdateOrderAsync(order);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for order {OrderNumber}", orderNumber);
                return false;
            }
        }
    }
}
