using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; } = "Success";
    }

}
