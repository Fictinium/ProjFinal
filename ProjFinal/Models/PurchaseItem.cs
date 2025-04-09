using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjFinal.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A {0} é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que 0.")]
        [Display(Name = "Quantidade")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "A {0} associada é obrigatória.")]
        [Display(Name = "Compra")]
        public int PurchaseId { get; set; }
        [ForeignKey("PurchaseId")]
        public Purchase Purchase { get; set; }

        [Required(ErrorMessage = "O {0} associado é obrigatório.")]
        [Display(Name = "Livro")]
        public int BookId { get; set; }
        [ForeignKey("BookId")]
        public Book Book { get; set; }

    }
}
