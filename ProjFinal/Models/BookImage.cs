using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjFinal.Models
{
    public class BookImage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A {0} é obrigatória.")]
        [Display(Name = "Imagem")]
        public string Image { get; set; }

        [Required(ErrorMessage = "O {0} é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O número da página deve ser maior que zero.")]
        [Display(Name = "Número da Página")]
        public int PageNumber { get; set; }

        [Required(ErrorMessage = "O {0} associado é obrigatório.")]
        [Display(Name = "Livro")]
        public int BookId { get; set; }
        [ForeignKey("BookId")]
        public Book Book { get; set; }
    }
}
