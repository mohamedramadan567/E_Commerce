using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.API.Controllers
{
    public class OrdersController : ApiBaseController
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        // POST /api/Orders/ Create Order => Order (BasketId - DeliveryMethod Id , Ship To Address) [Email]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderToReturnDto>> CreateOrder(OrderDto orderDto, CancellationToken ct)
        {
            return ToActionResult(await _orderService.CreateOrderAsync(orderDto, GetEmailFromToken(), ct));
        }
        // GET /api/Orders/Delivery Method => List Of Delivery Method
        // GET /api/Orders/ Orders Of User => [Email] -> User Orders For Logged In User
        // GET /api/Orders/{id}  Order Of User => Id + [Email] -> User Order For Logged In User
    }
}
