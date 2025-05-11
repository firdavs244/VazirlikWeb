using VazirlikWeb.Data;
using Microsoft.EntityFrameworkCore;
using VazirlikWeb.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity xizmatlarini qo'shish
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password talablari
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // User talablari
    options.User.RequireUniqueEmail = true;

    // Lockout sozlamalari
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie sozlamalari
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.SlidingExpiration = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication va Authorization middlewarelarini qo'shish
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Administrator va boshqa rollarni yaratish (ishga tushganda bir martalik)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

    // Ma'lumotlar bazasini yaratish yoki yangilash
    context.Database.Migrate();

    // Rollarni yaratish uchun metodni chaqirish
    await SeedRoles(roleManager);

    // Admin foydalanuvchisini yaratish uchun metodni chaqirish
    await SeedAdminUser(userManager, roleManager);
}

app.Run();

// Rollarni yaratish uchun metod
async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
{
    if (!await roleManager.RoleExistsAsync("Administrator"))
    {
        await roleManager.CreateAsync(new ApplicationRole
        {
            Name = "Administrator",
            Description = "Tizim administratori"
        });
    }

    if (!await roleManager.RoleExistsAsync("Editor"))
    {
        await roleManager.CreateAsync(new ApplicationRole
        {
            Name = "Editor",
            Description = "Yangiliklar muharriri"
        });
    }

    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new ApplicationRole
        {
            Name = "User",
            Description = "Oddiy foydalanuvchi"
        });
    }
}

// Admin foydalanuvchisini yaratish uchun metod
async Task SeedAdminUser(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
{
    // Admin foydalanuvchisini tekshirish va yaratish
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Administrator");
        }
    }
}