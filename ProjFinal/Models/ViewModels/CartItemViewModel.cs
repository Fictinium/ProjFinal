using System.ComponentModel.DataAnnotations;

namespace ProjFinal.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int BookId { get; set; }

        public string Title { get; set; }

        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que 0.")]
        public int Quantity { get; set; }
    }
}
