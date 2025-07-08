namespace ProjFinal.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string IdentityUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Purchase> Purchases { get; set; }

        public ICollection<Book> Books { get; set; }
    }
}
