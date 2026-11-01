
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RefrigeracionControl.Infrastructure.Data;
using RefrigeracionControl.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. CONFIGURACIÓN MVC
// ========================================

builder.Services.AddControllersWithViews();

// ========================================
// 2. CONEXIÓN A POSTGRESQL
// ========================================

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión DefaultConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// ========================================
// 3. CONFIGURACIÓN DE IDENTITY
// ========================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Usuarios
        options.User.RequireUniqueEmail = true;

        // Políticas de contraseña
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // Bloqueo por intentos fallidos
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ========================================
// 4. VALIDACIÓN DE SESIONES
// ========================================

// Comprueba el sello de seguridad en cada
// solicitud autenticada. Útil para invalidar
// sesiones al desactivar usuarios.

builder.Services.Configure<SecurityStampValidatorOptions>(
    options =>
    {
        options.ValidationInterval = TimeSpan.Zero;
    });

// ========================================
// 5. CONFIGURACIÓN DE COOKIES
// ========================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";

    options.AccessDeniedPath =
        "/Account/AccessDenied";

    options.Cookie.Name =
        "RefrigeracionControl.Auth";

    options.Cookie.HttpOnly = true;

    options.Cookie.SecurePolicy =
        CookieSecurePolicy.Always;

    options.Cookie.SameSite =
        SameSiteMode.Lax;

    options.ExpireTimeSpan =
        TimeSpan.FromHours(8);

    options.SlidingExpiration = true;
});

// ========================================
// 6. CREAR LA APLICACIÓN
// ========================================

var app = builder.Build();

// ========================================
// 7. MANEJO DE ERRORES Y HTTPS
// ========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// ========================================
// 8. AUTENTICACIÓN Y AUTORIZACIÓN
// ========================================

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// ========================================
// 9. RUTAS MVC
// ========================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Dashboard}/{action=Index}/{id?}");

// ========================================
// 10. INICIALIZACIÓN DE ROLES Y ADMIN
// ========================================

// Solo se ejecuta en desarrollo.
// IdentitySeeder debe existir en Infrastructure.

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    await IdentitySeeder.SeedAsync(
        scope.ServiceProvider,
        app.Configuration);
}

// ========================================
// 11. INICIAR SERVIDOR
// ========================================

app.Run();

