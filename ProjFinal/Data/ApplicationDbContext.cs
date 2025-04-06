using Microsoft.EntityFrameworkCore;

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
        }
    }

}
