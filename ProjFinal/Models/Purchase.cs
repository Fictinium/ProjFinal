using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjFinal.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        [Display(Name = "Preço Total")]
        public decimal TotalPrice { get; set; }

        [NotMapped]
        [Display(Name = "Preço Total")]
        [Required(ErrorMessage = "O {0} é obrigatório.")]
        [StringLength(15)]
        [RegularExpression("[0-9]{1,12}([,.][0-9]{1,2})?", ErrorMessage = "Só são aceites algarismos. Pode escrever duas casas decimais, separadas por . ou ,")]
        public string AuxTotalPrice { get; set; }

        [Display(Name = "Utilizador")]
        [Required(ErrorMessage = "O {0} associado é obrigatório.")]
        public string ConnectedUserId { get; set; }

        [Required(ErrorMessage = "A {0} é obrigatória.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Data de Compra")]
        public DateTime PurchaseDate { get; set; }

        public PurchaseStatus Status { get; set; }

        [Display(Name = "Livros")]
        public ICollection<PurchaseItem> Items { get; set; }
    }
}
