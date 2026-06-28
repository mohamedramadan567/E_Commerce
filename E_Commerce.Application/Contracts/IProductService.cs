using E_Commerce.Application.Common;
using E_Commerce.Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Contracts
{
    public interface IProductService
    {
        Task<Result<IReadOnlyList<ProductDto>>> GetAllProductsAsync(CancellationToken ct);
        Task<Result<IReadOnlyList<ProductDto>>> GetAllBrandsAsync(CancellationToken ct);
        Task<Result<IReadOnlyList<TypeDto>>> GetAllTypesAsync(CancellationToken ct);
        Task<Result<ProductDto>> GetProductByIdAsync(CancellationToken ct);
    }
}
