using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Models;

namespace ProjFinal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // herdar comportamento padrão
            base.OnModelCreating(modelBuilder);

            // criar o perfil de utilizador 'admin'
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "admin-role-id", Name = "admin", NormalizedName = "ADMIN" }
            );

            // criar um utilizador para funcionar como ADMIN
            var hasher = new PasswordHasher<ApplicationUser>();

            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = "admin-user-id",
                    UserName = "admin@digibook.pt",
                    NormalizedUserName = "ADMIN@DIGIBOOK.PT",
                    Email = "admin@digibook.pt",
                    NormalizedEmail = "ADMIN@DIGIBOOK.PT",
                    EmailConfirmed = true,
                    SecurityStamp = "b1e2c3d4-e5f6-7890-abcd-ef1234567890",
                    ConcurrencyStamp = "c2d3e4f5-6789-0123-abcd-ef2345678901",
                    FullName = "Admin DigiBook",
                    PasswordHash = "AQAAAAIAAYagAAAAECSZ2cwxYAlJPswTOYZAxHLkt7MM3C+7R1R/iqrDgsctjjEf2URmhOQ0dtnL1UbsFQ=="
                }
            );

            // associar utilizador à role ADMIN
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = "admin-user-id",
                    RoleId = "admin-role-id"
                }
            );
        }


        public DbSet<Book> Books { get; set; }
        public DbSet<BookImage> BookImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}