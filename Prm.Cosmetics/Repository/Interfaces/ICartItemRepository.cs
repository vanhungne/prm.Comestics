using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface ICartItemRepository
    {
        Task<List<CartItem>> GetCartItemsByUserIdAsync(int userId);
        Task DeleteCartItemsAsync(List<CartItem> cartItems);
        Task<CartItem> GetCartItemAsync(int userId, int productId);
        Task<CartItem> AddCartItemAsync(CartItem cartItem);
        Task<CartItem> UpdateCartItemAsync(CartItem cartItem);
        Task<bool> DeleteCartItemAsync(CartItem cartItem);
        Task<bool> DeleteCartItemByIdAsync(int userId, int productId);
        Task<int> GetCartItemCountAsync(int userId);
        Task<decimal> GetCartTotalAsync(int userId);
        Task<bool> ExistsAsync(int userId, int productId);
    }
}
