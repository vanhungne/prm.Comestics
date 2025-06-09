using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Model.CheckOut
{
    public class CheckoutRequest
    {
        //public int UserId { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public List<CheckoutItem> Items { get; set; } = new List<CheckoutItem>();
    }
    public class CheckoutItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CheckoutResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public bool RequiresPaymentRedirect { get; set; } = false;
    }
}
