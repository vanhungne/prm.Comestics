using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Model;
using Service.Interface;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class productsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<productsController> _logger;

        public productsController(IProductService productService, ILogger<productsController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all products with optional search and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductSearchDto searchDto = null)
        {
            try
            {
                // If no search criteria provided, use default values
                if (searchDto == null)
                {
                    searchDto = new ProductSearchDto();
                }

                // Limit max page size
                if (searchDto.PageSize > 100)
                {
                    searchDto.PageSize = 100;
                }

                // Ensure minimum page size
                if (searchDto.PageSize <= 0)
                {
                    searchDto.PageSize = 10;
                }

                // Ensure minimum page number
                if (searchDto.Page <= 0)
                {
                    searchDto.Page = 1;
                }

                var result = await _productService.GetAllProductsAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, "An error occurred while retrieving products.");
            }
        }


        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID {ProductId}", id);
                return StatusCode(500, "An error occurred while retrieving the product.");
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto createDto, IFormFile imageFile = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _productService.CreateProductAsync(createDto, imageFile);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "An error occurred while creating the product.");
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromForm] UpdateProductDto updateDto, IFormFile imageFile = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _productService.UpdateProductAsync(id, updateDto, imageFile);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
                return StatusCode(500, "An error occurred while updating the product.");
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
                return StatusCode(500, "An error occurred while deleting the product.");
            }
        }

        /// <summary>
        /// Get products with low stock
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts([FromQuery] int threshold = 10)
        {
            try
            {
                var products = await _productService.GetLowStockProductsAsync(threshold);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock products");
                return StatusCode(500, "An error occurred while retrieving low stock products.");
            }
        }
    }
}