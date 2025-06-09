using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IPaypalService
    {
        Task<string> CreatePaymentAsync(decimal amount, int orderNumber);
        Task<(string Status, string OrderId)> CapturePaymentAsync(string token);
    }
}
