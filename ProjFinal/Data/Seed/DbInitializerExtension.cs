namespace ProjFinal.Data.Seed
{
    internal static class DbInitializerExtension
    {
        public static async Task<IApplicationBuilder> UseItToSeedSqlServerAsync(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                await DbInitializer.Initialize(context, services);
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Exception during DB seed:");
                Console.WriteLine(ex.ToString());
                throw;
            }

            return app;
        }
    }
}

