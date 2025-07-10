using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProjFinal.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [StringLength(100)]
        [Display(Name = "Nome Completo")]
        public string FullName { get; set; }
    }
}
