using E_Commerce.API.Attributes;
using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace E_Commerce.API.Controllers
{
    public class ProductsController : ApiBaseController
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }
        //Get All Products
        [HttpGet]
        [RedisCache(90)]
        public async Task<ActionResult<PaginatedResult<ProductDto>>> GetAllProducts([FromQuery]ProductQueryParams QueryParams, CancellationToken ct)
        {
            var products = await _service.GetAllProductsAsync(QueryParams, ct);
            return ToActionResult(products);
        }
        //Get Product By Id
        [HttpGet("{id}")]
        //عشان تعرف سواجر ان فيه شكل تاني من الرد غير 200 اوك
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProduct(int id, CancellationToken ct)
        {
            var result = await _service.GetProductByIdAsync(id, ct);
            return ToActionResult(result);
        }
        //Get All Types
        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<TypeDto>>> GetAllTypes(CancellationToken ct)
        {
            return ToActionResult(await _service.GetAllTypesAsync(ct));
        }
        //Get All Brands
        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<BrandDto>>> GetAllBrands(CancellationToken ct)
        {
            return ToActionResult(await _service.GetAllBrandsAsync(ct));
        }
    }
}
