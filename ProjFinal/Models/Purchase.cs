using System.ComponentModel.DataAnnotations;

namespace ProjFinal.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O {0} é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço total deve ser maior que 0 euros.")]
        [Display(Name = "Preço Total")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "A {0} é obrigatória.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Data de Compra")]
        public DateTime PurchaseDate { get; set; }

        public PurchaseStatus Status { get; set; }

        [Display(Name = "Livros")]
        public ICollection<PurchaseItem> Items { get; set; }
    }
}
