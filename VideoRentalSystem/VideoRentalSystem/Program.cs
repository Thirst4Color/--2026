using Microsoft.EntityFrameworkCore;
using VideoRentalSystem.Models;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// ВАЖНО: Добавьте поддержку MVC с представлениями
builder.Services.AddControllersWithViews(); // ← ДОБАВЬТЕ ЭТУ СТРОКУ

// Для API (можно оставить)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Подключение базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Обработка миграций и начальных данных
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Применяем миграции или создаем базу
        dbContext.Database.EnsureCreated();

        // Заполняем данные
        SeedData(dbContext);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка при инициализации базы данных: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Video Rental API v1");
        c.RoutePrefix = "api-docs"; // Перенесем Swagger сюда
    });

    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ← ВАЖНО: для обслуживания CSS/JS файлов
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

// ВАЖНО: Настройка маршрутов
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Также оставляем API маршруты
app.MapControllers();

app.Run();

// Метод для заполнения начальными данными
static void SeedData(ApplicationDbContext context)
{
    // ... ваш существующий код SeedData ...
}