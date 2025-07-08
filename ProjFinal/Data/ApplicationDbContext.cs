using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Models;

namespace ProjFinal.Data
{
    namespace YourProjectNamespace.Data
    {
        public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public DbSet<Book> Books { get; set; }
            public DbSet<BookImage> BookImages { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<Purchase> Purchases { get; set; }
            public DbSet<PurchaseItem> PurchaseItems { get; set; }
            public DbSet<User> Users { get; set; }
        }
    }

}
