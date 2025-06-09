using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repository.Entities;
using Repository.Interface;
using Repository.Model;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICloundinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;
        public ProductService(IProductRepository productRepository, ICloundinaryService cloudinaryService, IMapper mapper, ILogger<ProductService> logger)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<ProductDto>> GetAllProductsAsync(ProductSearchDto searchDto = null)
        {
            try
            {
                var result = await _productRepository.GetAllAsync(searchDto);
                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(result.Items);

                return new PagedResult<ProductDto>
                {
                    Items = productDtos,
                    TotalCount = result.TotalCount,
                    Page = result.Page,
                    PageSize = result.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all products");
                throw;
            }
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found.");
                }

                return _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product with ID {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto, IFormFile imageFile = null)
        {
            try
            {
                var product = _mapper.Map<Product>(createDto);

                // Upload image if provided
                if (imageFile != null)
                {
                    product.ImageUrl = await _cloudinaryService.UploadImage(imageFile);
                }

                var createdProduct = await _productRepository.CreateAsync(product);
                _logger.LogInformation("Product created successfully with ID {ProductId}", createdProduct.Id);

                return _mapper.Map<ProductDto>(createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product");
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateDto, IFormFile imageFile = null)
        {
            try
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found.");
                }

                // Update properties if provided
                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                {
                    existingProduct.Name = updateDto.Name;
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Description))
                {
                    existingProduct.Description = updateDto.Description;
                }

                if (updateDto.Price.HasValue)
                {
                    existingProduct.Price = updateDto.Price.Value;
                }

                if (updateDto.StockQuantity.HasValue)
                {
                    existingProduct.StockQuantity = updateDto.StockQuantity.Value;
                }

                // Upload new image if provided
                if (imageFile != null)
                {
                    existingProduct.ImageUrl = await _cloudinaryService.UploadImage(imageFile);
                }

                var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
                _logger.LogInformation("Product updated successfully with ID {ProductId}", id);

                return _mapper.Map<ProductDto>(updatedProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product with ID {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var exists = await _productRepository.ExistsAsync(id);
                if (!exists)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found.");
                }

                var result = await _productRepository.DeleteAsync(id);
                if (result)
                {
                    _logger.LogInformation("Product deleted successfully with ID {ProductId}", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product with ID {ProductId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10)
        {
            try
            {
                var products = await _productRepository.GetLowStockProductsAsync(threshold);
                return _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting low stock products");
                throw;
            }
        }
    }
}