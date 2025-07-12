using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ProjFinal.Data;
using ProjFinal.Data.Seed;
using ProjFinal.Models;
using ProjFinal.Services;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.  
builder.Services.AddControllersWithViews();

// Ler do ficheiro 'appsettings.json' os dados da BD  
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// Define o tipo de BD e a sua ligação
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configurar o uso do IdentityUser como 'utilizador' de autenticação
// Se não se adicionar à instrução '.AddRoles' não é possível usar os ROLES
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
   .AddRoles<IdentityRole>()
   .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDistributedMemoryCache();
// configurar o de uso de 'cookies'
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer("JwtBearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });


// configuração do JWT
builder.Services.AddScoped<JwtService>();

// Potencialmente necessário para a adição de imagens/PDFs
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 314572800; // 300MB
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

// If you place app.UseAuthorization(); before app.UseAuthentication();, it will not detect users correctly, leading to unauthorized errors.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "search",
    pattern: "pesquisa",
    defaults: new { controller = "Search", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

await app.UseItToSeedSqlServerAsync();

app.Run();
