using E_Commerce.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace E_Commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }
        //Get All Products
        [HttpGet]
        public async Task<ActionResult> GetAllProducts(CancellationToken ct)
        {
            var products = await _service.GetAllProductsAsync(ct);
            return Ok(products);
        }
        //Get Product By Id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id, CancellationToken ct)
        {
            var result = await _service.GetProductByIdAsync(id, ct);
            return Ok(result);
        }
        //Get All Types
        [HttpGet("types")]
        public async Task<ActionResult> GetAllTypes(CancellationToken ct)
        {
            return Ok(await _service.GetAllTypesAsync(ct));
        }
        //Get All Brands
        [HttpGet("brands")]
        public async Task<ActionResult> GetAllBrands(CancellationToken ct)
        {
            return Ok(await _service.GetAllBrandsAsync(ct));
        }
    }
}
