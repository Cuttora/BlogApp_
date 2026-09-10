using BlogApp.Data;
using BlogApp.Models;
using BlogApp.Services;
using BlogApp.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

// ---- Инициализация NLog ДО создания builder ----
var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
{
    logger.Info("Приложение запускается");

    var builder = WebApplication.CreateBuilder(args);

    // Подключаем NLog как провайдер логирования
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // ---- Data Access layer: EF Core + SQLite ----
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=app.db";

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));

    // ---- Authentication / Authorization: ASP.NET Core Identity ----
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;

        options.User.RequireUniqueEmail = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

    // ---- Business Logic layer: application services ----
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IArticleService, ArticleService>();
    builder.Services.AddScoped<ITagService, TagService>();
    builder.Services.AddScoped<ICommentService, CommentService>();

    // ---- Логгер действий пользователя ----
    builder.Services.AddScoped<UserActionLogger>();

    // ---- Presentation layer: MVC ----
    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // ---- Apply migrations and seed initial data (roles + demo users) ----
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();

            await SeedData.InitializeAsync(services);
            logger.Info("База данных успешно инициализирована");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Ошибка при инициализации базы данных");
            throw;
        }
    }

    // ---- HTTP request pipeline ----

    // Глобальный обработчик исключений — включаем во всех окружениях,
    // чтобы пользователь всегда попадал на страницу «Что-то пошло не так».
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionFeature?.Error;

            // Логируем необработанное исключение (ILogger уже пишет в NLog)
            var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
            var errorLogger = loggerFactory.CreateLogger("GlobalExceptionHandler");
            errorLogger.LogError(exception,
                "Необработанное исключение на {Path}",
                context.Request.Path);

            // Перенаправляем пользователя на страницу «Что-то пошло не так»
            context.Response.Redirect("/Home/Error");
            await Task.CompletedTask;
        });
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Article}/{action=Index}/{id?}");

    logger.Info("Приложение готово к приёму запросов");
    app.Run();
}
catch (Exception ex)
{
    // Логируем фатальную ошибку при запуске приложения
    logger.Error(ex, "Приложение упало при запуске");
    throw;
}
finally
{
    // Корректно завершаем NLog, чтобы все логи были записаны на диск
    LogManager.Shutdown();
}