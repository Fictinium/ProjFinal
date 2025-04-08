using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjFinal.Models
{
    public class BookImage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O URL da imagem é obrigatório.")]
        [Url(ErrorMessage = "Insira um URL válido para a imagem.")]
        [Display(Name = "URL da Imagem")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "O número da página é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O número da página deve ser maior que zero.")]
        [Display(Name = "Número da Página")]
        public int PageNumber { get; set; }

        [Required(ErrorMessage = "O livro associado é obrigatório.")]
        [Display(Name = "Livro")]
        public int BookId { get; set; }
        [ForeignKey("BookId")]
        public Book Book { get; set; }
    }
}
