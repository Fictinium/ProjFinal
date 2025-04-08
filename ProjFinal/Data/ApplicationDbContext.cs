using Microsoft.EntityFrameworkCore;
using ProjFinal.Models;

namespace ProjFinal.Data
{
    namespace YourProjectNamespace.Data
    {
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }
            public DbSet<Book> Books { get; set; }
            public DbSet<Book> BookImage { get; set; }
            public DbSet<Book> Category { get; set; }
            public DbSet<Book> Purchase { get; set; }
            public DbSet<Book> PurchaseItem { get; set; }
        }
    }

}
