using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Application.DTOs.Baskets
{
    public class BasketItemDto
    {
        [Required(ErrorMessage = "Product Id Is Required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name Is Required ")]
        public string ProductName { get; set; } = default!;

        public string PictureUrl { get; set; } = default!;

        [Range(1, double.MaxValue)]
        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}