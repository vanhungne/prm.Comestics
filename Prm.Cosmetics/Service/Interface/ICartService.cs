using Repository.Model.Cartt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface ICartService
    {
        Task<CartResponse> AddToCartAsync(int userId, int productId, int quantity);
        Task<CartResponse> UpdateCartItemAsync(int userId, int productId, int quantity);
        Task<CartResponse> RemoveFromCartAsync(int userId, int productId);
        Task<CartResponse> GetCartAsync(int userId);
        Task<CartResponse> ClearCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
        Task<decimal> GetCartTotalAsync(int userId);
    }
}
