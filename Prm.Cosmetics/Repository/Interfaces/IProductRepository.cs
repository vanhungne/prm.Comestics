using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IProductRepository
    {
        Task<PagedResult<Product>> GetAllAsync(ProductSearchDto searchDto = null);
        Task<Product> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
        Task<List<Product>> GetProductsByIdsAsync(List<int> productIds);
    }
}