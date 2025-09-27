using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PC2_PatrickPonce.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "PortalInmobiliario_";
});

builder.Services.AddSession(options =>
{
    // Define el tiempo de inactividad de la sesión
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Hace que la cookie de sesión sea esencial para el funcionamiento de la app
});

var app = builder.Build();

app.UseSession(); // Habilita el middleware de sesión
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

// Crear el rol "Broker" si no existe
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    if (!await roleManager.RoleExistsAsync("Broker"))
    {
        await roleManager.CreateAsync(new IdentityRole("Broker"));
    }

    // Opcional: Asignar el rol a un usuario de prueba
    var testUser = await userManager.FindByEmailAsync("broker@example.com");
    if (testUser == null)
    {
        testUser = new IdentityUser { UserName = "broker@example.com", Email = "broker@example.com", EmailConfirmed = true };
        await userManager.CreateAsync(testUser, "TestPass123!");
        await userManager.AddToRoleAsync(testUser, "Broker");
    }
}

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
