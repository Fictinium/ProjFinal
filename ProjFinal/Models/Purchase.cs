using System.ComponentModel.DataAnnotations;

namespace ProjFinal.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O preço total é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço total deve ser maior que 0 euros.")]
        [Display(Name = "Preço Total")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "A data de compra é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de Compra")]
        public DateTime PurchaseDate { get; set; }

        public PurchaseStatus Status { get; set; }

        [Display(Name = "Livros")]
        public ICollection<PurchaseItem> Items { get; set; }
    }
}
