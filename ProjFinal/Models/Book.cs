using System.ComponentModel.DataAnnotations;

namespace ProjFinal.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título do livro é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título não pode ultrapassar 150 caracteres.")]
        [Display(Name = "Título")]
        public string Title { get; set; }

        [Required(ErrorMessage = "O nome do autor é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome do autor não pode ultrapassar 100 caracteres.")]
        [Display(Name = "Autor")]
        public string Author { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(2000, ErrorMessage = "A descrição não pode ultrapassar 2000 caracteres.")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que 0 euros.")]
        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "A data de publicação é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de Publicação")]
        public DateTime PublishedDate { get; set; }

        [Required(ErrorMessage = "O URL do livro é obrigatório.")]
        [Url(ErrorMessage = "Insira um URL válido.")]
        [Display(Name = "URL do Livro")]
        public string FileUrl { get; set; }

        [Display(Name = "Imagens")]
        public ICollection<BookImage> Images { get; set; }

        [Display(Name = "Categorias")]
        public ICollection<Category> Categories { get; set; }

    }
}
