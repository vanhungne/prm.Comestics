using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Entities;
using Repository.Interface;
using Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ComesticDbContext _context;
        public ProductRepository(ComesticDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Product>> GetProductsByIdsAsync(List<int> productIds)
        {
            return await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
        }
        public async Task<PagedResult<Product>> GetAllAsync(ProductSearchDto searchDto)
        {
            var query = _context.Products.AsQueryable();

            // Apply search filters only if search criteria are provided
            if (!string.IsNullOrWhiteSpace(searchDto?.SearchTerm))
            {
                var searchTerm = searchDto.SearchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm) ||
                                        p.Description.ToLower().Contains(searchTerm));
            }

            if (searchDto?.MinPrice.HasValue == true)
            {
                query = query.Where(p => p.Price >= searchDto.MinPrice.Value);
            }

            if (searchDto?.MaxPrice.HasValue == true)
            {
                query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);
            }

            if (searchDto?.InStock.HasValue == true)
            {
                if (searchDto.InStock.Value)
                {
                    query = query.Where(p => p.StockQuantity > 0);
                }
                else
                {
                    query = query.Where(p => p.StockQuantity == 0);
                }
            }

            // Apply sorting (with default values if searchDto is null)
            var sortBy = searchDto?.SortBy ?? "CreatedAt";
            var sortOrder = searchDto?.SortOrder ?? "desc";
            query = ApplySorting(query, sortBy, sortOrder);

            var totalCount = await query.CountAsync();

            // Apply pagination (with default values if searchDto is null)
            var page = searchDto?.Page ?? 1;
            var pageSize = searchDto?.PageSize ?? 10;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }


        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _context.Products
                .Where(p => p.StockQuantity <= threshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            Expression<Func<Product, object>> keySelector = sortBy?.ToLower() switch
            {
                "name" => p => p.Name,
                "price" => p => p.Price,
                "stockquantity" => p => p.StockQuantity,
                "createdat" => p => p.CreatedAt,
                _ => p => p.CreatedAt
            };

            return isDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}

