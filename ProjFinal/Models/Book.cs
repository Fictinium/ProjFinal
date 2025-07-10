using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjFinal.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O {0} do livro é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título não pode ultrapassar 150 caracteres.")]
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome do {0} é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome do autor não pode ultrapassar 100 caracteres.")]
        [Display(Name = "Autor")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "A {0} é obrigatória.")]
        [StringLength(2000, ErrorMessage = "A descrição não pode ultrapassar 2000 caracteres.")]
        [Display(Name = "Descrição")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        [NotMapped]
        [Display(Name = "Preço")]
        [Required(ErrorMessage = "O {0} é obrigatório.")]
        [StringLength(15)]
        [RegularExpression("[0-9]{1,12}([,.][0-9]{1,2})?", ErrorMessage = "Só são aceites algarismos. Pode escrever duas casas decimais, separadas por . ou ,")]
        public string AuxPrice { get; set; } = string.Empty;

        [Required(ErrorMessage = "A {0} é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de Publicação")]
        public DateTime PublishedDate { get; set; }

        [Required(ErrorMessage = "O {0} é obrigatório.")]
        [Display(Name = "Ficheiro do Livro")]
        public string BookFile { get; set; } = string.Empty;

        [Display(Name = "Imagens")]
        public ICollection<BookImage> Images { get; set; } = new List<BookImage>();

        [Display(Name = "Categorias")]
        public ICollection<Category> Categories { get; set; } = new List<Category>();

    }
}
