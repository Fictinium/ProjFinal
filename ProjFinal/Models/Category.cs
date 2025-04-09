using System.ComponentModel.DataAnnotations;

namespace ProjFinal.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O {0} da categoria é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode ultrapassar 100 caracteres.")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required(ErrorMessage = "A {0} da categoria é obrigatória.")]
        [StringLength(1000, ErrorMessage = "A descrição não pode ultrapassar 1000 caracteres.")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Livros")]
        public ICollection<Book> Books { get; set; }
    }

}
