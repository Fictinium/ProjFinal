using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjFinal.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O {0} é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode ultrapassar 100 caracteres.")]
        [Display(Name = "Nome Completo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O {0} é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "A {0} é obrigatória.")]
        [StringLength(100)]
        [Display(Name = "Palavra-passe")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Utilizador Identity")]
        public string IdentityUserId { get; set; } = string.Empty;
        [ForeignKey(nameof(IdentityUserId))]
        public ApplicationUser? ApplicationUser { get; set; }

        [Display(Name = "Compras")]
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

        [Display(Name = "Livros")]
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
