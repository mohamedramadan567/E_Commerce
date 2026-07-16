using E_Commerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Domain.Entities.Orders
{
    //Table
    public class Order : BaseEntity<Guid>
    {
        //Ef Core
        private Order()
        {
            
        }
        public Order(string buyerEmail, OrderAddress shipToAddress, ICollection<OrderItem> items, DeliveryMethod deliveryMethod, decimal subTotal)
        {
            BuyerEmail = buyerEmail;
            ShipToAddress = shipToAddress;
            Items = items;
            DeliveryMethod = deliveryMethod;
            SubTotal = subTotal;
        }

        public string BuyerEmail { get; set; } = default!;
        public OrderAddress ShipToAddress { get; set; } = default!;
        public ICollection<OrderItem> Items { get; set; } = [];
        public DeliveryMethod DeliveryMethod { get; set; } = default!;
        public decimal SubTotal { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public int DeliveryMethodId { get; set; } // FK
        public decimal GetTotal() => SubTotal + (DeliveryMethod?.Cost ?? 0m);
    }
}
