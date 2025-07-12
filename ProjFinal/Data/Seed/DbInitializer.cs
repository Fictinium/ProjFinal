using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;

namespace ProjFinal.Data.Seed
{
    internal class DbInitializer
    {
        internal static async Task Initialize(ApplicationDbContext context, IServiceProvider services)
        {
            ArgumentNullException.ThrowIfNull(context);
            await context.Database.MigrateAsync();

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            const string adminRoleName = "admin";
            const string adminEmail = "admin@digibook.com";
            const string adminPassword = "Admin123_";

            // Ensure the role exists
            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRoleName));
            }

            // Check if admin user exists
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            ApplicationUser adminUser;

            if (existingAdmin == null)
            {
                // Create new admin user
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("N"),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    FullName = "Admin DigiBook"
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (!createResult.Succeeded)
                {
                    throw new Exception($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                adminUser = existingAdmin;

                // Update details if needed
                adminUser.FullName = "Admin DigiBook";
                adminUser.EmailConfirmed = true;
                await userManager.UpdateAsync(adminUser);
            }

            // Ensure the admin has the role
            if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }

            // Update or create admin profile
            var adminProfile = await context.UserProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == adminUser.Id);
            if (adminProfile == null)
            {
                adminProfile = new UserProfile
                {
                    Name = "Admin User",
                    Email = adminUser.Email,
                    IdentityUserId = adminUser.Id
                };
                await context.UserProfiles.AddAsync(adminProfile);
            }
            else
            {
                adminProfile.Name = "Admin User";
                adminProfile.Email = adminUser.Email;
                context.UserProfiles.Update(adminProfile);
            }

            // Seed categories (only if missing)
            if (!context.Categories.Any())
            {
                // Criar categorias
                var categories = Array.Empty<Category>();

                categories = new[]
                {
                    new Category { Name = "Drama", Description = "Literary works meant for performance, focusing on emotional development and conflict through dialogue and action." },
                    new Category { Name = "Philosophy", Description = "Texts that explore fundamental questions about existence, morality, knowledge, and the human condition." },
                    new Category { Name = "Classic Literature", Description = "Canonical works known for their lasting cultural value, literary merit, and historical significance." },
                    new Category { Name = "German Literature", Description = "Works originating from German-speaking countries, often rich in intellectual depth and philosophical influence." },
                    new Category { Name = "Adventure Fiction", Description = "Stories featuring exploration, risk, and physical action, often in exotic or dangerous settings." },
                    new Category { Name = "Epic Novel", Description = "A long, grand-scale narrative focusing on heroic or existential quests, often spanning vast time or space." },
                    new Category { Name = "American Classic", Description = "Seminal literature from the United States that reflects its culture, values, and social evolution." },
                    new Category { Name = "Maritime Literature", Description = "Narratives centered around the sea, sailors, whaling, or naval life, often exploring isolation and nature." },
                    new Category { Name = "Novella", Description = "A work of fiction longer than a short story but shorter than a novel, usually focused and symbolic." },
                    new Category { Name = "Absurdist Fiction", Description = "Works that depict the human struggle to find meaning in a chaotic, illogical world." },
                    new Category { Name = "Modernist Literature", Description = "Early 20th-century works that experiment with narrative, form, and realism, often reflecting disillusionment." },
                    new Category { Name = "Kafkaesque", Description = "Fiction characterized by surreal, oppressive, or nightmarish situations, often involving bureaucracy or alienation." },
                    new Category { Name = "Gothic Fiction", Description = "A genre combining horror, romance, and the supernatural, often set in dark or mysterious locations." },
                    new Category { Name = "Romance", Description = "Stories centered around emotional relationships, love, and personal connection, with varying levels of conflict." },
                    new Category { Name = "British Literature", Description = "Literary works originating from the UK, encompassing a wide range of styles, periods, and social commentary." }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }


            // Seed books (only if missing)
            if (!context.Books.Any())
            {
                var categories = await context.Categories.ToListAsync();

                // Criar livros
                var book1 = new Book
                {
                    Title = "Faust",
                    Author = "Johann Wolfgang von Goethe",
                    Description = "A two-part dramatic poem exploring the life of Faust, a disillusioned scholar who makes a pact with the devil, Mephistopheles. Part I focuses on Faust's despair, his deal, and love for Gretchen; Part II reaches across myth and redemption.",
                    Price = 4.99m,
                    AuxPrice = "4,99",
                    PublishedDate = new DateTime(2005, 1, 4),
                    BookFile = "Books/faust-58d8b450/book_8442c2cf-2a18-46bb-9fb3-b26ea0cfacea.pdf",
                    Images = new List<BookImage>
                    {
                        new BookImage { PageNumber = 1, Image = "faust-58d8b450/BookImages/page_1.jpg" }
                    },
                    Categories = new List<Category> {
                        categories.FirstOrDefault(c => c.Name == "Drama")!,
                        categories.FirstOrDefault(c => c.Name == "Philosophy")!,
                        categories.FirstOrDefault(c => c.Name == "Classic Literature")!,
                        categories.FirstOrDefault(c => c.Name == "German Literature")!
                    }
                };

                var book2 = new Book
                {
                    Title = "The Metamorphosis",
                    Author = "Franz Kafka",
                    Description = "Salesman Gregor Samsa wakes to find himself transformed into an insect, struggling with his new identity and alienation. A haunting exploration of isolation, identity, and familial bonds.",
                    Price = 5.99m,
                    AuxPrice = "5,99",
                    PublishedDate = new DateTime(2005, 8, 17),
                    BookFile = "Books/the-metamorphosis-0320d5ed/book_a8c06f28-2089-4f86-bc03-6fb0abef2649.pdf",
                    Images = new List<BookImage>
                    {
                        new BookImage { PageNumber = 1, Image = "the-metamorphosis-0320d5ed/BookImages/page_1.png" }
                    },
                    Categories = new List<Category>{
                        categories.FirstOrDefault(c => c.Name == "Novella")!,
                        categories.FirstOrDefault(c => c.Name == "Absurdist Fiction")!,
                        categories.FirstOrDefault(c => c.Name == "Modernist Literature")!,
                        categories.FirstOrDefault(c => c.Name == "Kafkaesque")!
                    }
                };

                await context.Books.AddRangeAsync(book1, book2);
            }

            await context.SaveChangesAsync();
        }
    }
}

