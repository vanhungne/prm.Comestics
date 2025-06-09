using Microsoft.AspNetCore.Http;
using Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IProductService
    {
        Task<PagedResult<ProductDto>> GetAllProductsAsync(ProductSearchDto searchDto = null);
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createDto, IFormFile imageFile = null);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateDto, IFormFile imageFile = null);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10);
    }
}
