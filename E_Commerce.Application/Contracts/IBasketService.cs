using E_Commerce.Application.Common;
using E_Commerce.Application.DTOs.Baskets;
using E_Commerce.Domain.Entities.Baskets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Contracts
{
    public interface IBasketService
    {
        //Get Basket => Take BasketId, Return BasketDto
        Task<Result<BasketDto>> GetBasketAsync(string basketId, CancellationToken ct = default);
        
        //Create or update Basket => Take Basket, Return Basket After Creation or update
        Task<Result<BasketDto>> CreateOrUpdateBasketAsync(BasketDto basket, TimeSpan? TLV = default, CancellationToken ct = default);

        //Delete Basket -> BasketId -> bool
        Task<Result<bool>> DeleteBasketAsync(string basketId, CancellationToken ct = default);
    }
}
