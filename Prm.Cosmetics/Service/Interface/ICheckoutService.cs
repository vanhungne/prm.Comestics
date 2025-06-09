using Repository.Entities;
using Repository.Model.CheckOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface ICheckoutService
    {
        Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request,int userId);
        Task<CheckoutResponse> CheckoutFromCartAsync(int userId, string paymentMethod = "Cash");
        Task<Order> GetOrderDetailsAsync(int orderId);
        Task<bool> ValidateStockAvailabilityAsync(List<CheckoutItem> items);
        Task<CheckoutResponse> CompletePayPalPaymentAsync(int orderId, string paypalTransactionId);
        Task<bool> ClearCartAfterPayPalPaymentAsync(int userId, int orderId);
    }
}
