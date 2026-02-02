using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using VideoRentalSystem.Models;
using VideoRentalSystem.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
        dbContext.Database.Migrate(); // Используйте вместо EnsureCreated() для миграций

        // Заполняем данные
        SeedData(dbContext);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка при инициализации базы данных: {ex.Message}");
    // Можно добавить логирование в файл
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Video Rental API v1");
        c.RoutePrefix = "swagger"; // Доступ по /swagger
    });

    // Подробное логирование для разработки
    app.UseDeveloperExceptionPage();
}
else
{
    // Обработка ошибок для production
    app.UseExceptionHandler("/error");
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Глобальный обработчик ошибок
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Необработанное исключение: {ex}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Внутренняя ошибка сервера");
    }
});

app.Run();

// Метод для заполнения начальными данными
static void SeedData(ApplicationDbContext context)
{
    // Проверяем, есть ли уже данные
    if (!context.MediaTypes.Any())
    {
        Console.WriteLine("Заполнение базы данных начальными данными...");

        // Добавляем типы носителей
        var mediaTypes = new List<MediaType>
        {
            new MediaType { Name = "VHS", DailyRentalPrice = 1.50m },
            new MediaType { Name = "DVD", DailyRentalPrice = 2.00m },
            new MediaType { Name = "Blu-Ray", DailyRentalPrice = 2.50m },
            new MediaType { Name = "HD-DVD", DailyRentalPrice = 2.20m }
        };
        context.MediaTypes.AddRange(mediaTypes);
        context.SaveChanges();
        Console.WriteLine("Добавлены типы носителей.");

        // Добавляем поставщиков
        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                Name = "VideoDistributor Inc.",
                ContactPerson = "Иван Петров",
                Phone = "+7 (123) 456-78-90",
                Email = "contact@videodistributor.com",
                Address = "г. Москва, ул. Кирова, д. 15"
            },
            new Supplier
            {
                Name = "FilmSupply Co.",
                ContactPerson = "Мария Сидорова",
                Phone = "+7 (987) 654-32-10",
                Email = "info@filmsupply.com",
                Address = "г. Санкт-Петербург, Невский пр., д. 25"
            }
        };
        context.Suppliers.AddRange(suppliers);
        context.SaveChanges();
        Console.WriteLine("Добавлены поставщики.");

        // Добавляем несколько фильмов
        var movies = new List<Movie>
        {
            new Movie
            {
                Title = "Крестный отец",
                Description = "Эпическая история сицилийской мафиозной семьи Корлеоне.",
                Year = 1972,
                Genre = "Криминал, Драма",
                Director = "Фрэнсис Форд Коппола",
                Duration = 175,
                Rating = 9.2m
            },
            new Movie
            {
                Title = "Побег из Шоушенка",
                Description = "Два заключенных заводят дружбу в тюрьме.",
                Year = 1994,
                Genre = "Драма",
                Director = "Фрэнк Дарабонт",
                Duration = 142,
                Rating = 9.3m
            },
            new Movie
            {
                Title = "Зеленая миля",
                Description = "История тюремного надзирателя и его необычного заключенного.",
                Year = 1999,
                Genre = "Драма, Фэнтези",
                Director = "Фрэнк Дарабонт",
                Duration = 189,
                Rating = 9.1m
            },
            new Movie
            {
                Title = "Форрест Гамп",
                Description = "История человека с низким IQ, который стал свидетелем ключевых событий XX века.",
                Year = 1994,
                Genre = "Драма, Комедия",
                Director = "Роберт Земекис",
                Duration = 142,
                Rating = 8.8m
            }
        };
        context.Movies.AddRange(movies);
        context.SaveChanges();
        Console.WriteLine($"Добавлены фильмы.");

        // Теперь добавляем медиа-носители для фильмов
        var mediaItems = new List<MediaItem>
        {
            // Крестный отец
            new MediaItem
            {
                MovieId = 1,
                MediaTypeId = 2, // DVD
                SupplierId = 1,
                Barcode = "DVD001",
                PurchaseDate = DateTime.Now.AddMonths(-6),
                PurchasePrice = 15.99m,
                IsAvailable = true,
                Condition = "Good"
            },
            new MediaItem
            {
                MovieId = 1,
                MediaTypeId = 3, // Blu-Ray
                SupplierId = 1,
                Barcode = "BR001",
                PurchaseDate = DateTime.Now.AddMonths(-3),
                PurchasePrice = 25.99m,
                IsAvailable = true,
                Condition = "Excellent"
            },
            // Побег из Шоушенка
            new MediaItem
            {
                MovieId = 2,
                MediaTypeId = 2, // DVD
                SupplierId = 2,
                Barcode = "DVD002",
                PurchaseDate = DateTime.Now.AddMonths(-5),
                PurchasePrice = 12.99m,
                IsAvailable = true,
                Condition = "Good"
            },
            new MediaItem
            {
                MovieId = 2,
                MediaTypeId = 1, // VHS
                SupplierId = 2,
                Barcode = "VHS001",
                PurchaseDate = DateTime.Now.AddYears(-1),
                PurchasePrice = 8.99m,
                IsAvailable = true,
                Condition = "Good"
            },
            // Зеленая миля
            new MediaItem
            {
                MovieId = 3,
                MediaTypeId = 2, // DVD
                SupplierId = 1,
                Barcode = "DVD003",
                PurchaseDate = DateTime.Now.AddMonths(-4),
                PurchasePrice = 14.99m,
                IsAvailable = true,
                Condition = "Excellent"
            },
            // Форрест Гамп
            new MediaItem
            {
                MovieId = 4,
                MediaTypeId = 3, // Blu-Ray
                SupplierId = 2,
                Barcode = "BR002",
                PurchaseDate = DateTime.Now.AddMonths(-2),
                PurchasePrice = 22.99m,
                IsAvailable = true,
                Condition = "Excellent"
            },
            new MediaItem
            {
                MovieId = 4,
                MediaTypeId = 2, // DVD
                SupplierId = 2,
                Barcode = "DVD004",
                PurchaseDate = DateTime.Now.AddMonths(-3),
                PurchasePrice = 16.99m,
                IsAvailable = false, // Выдана в прокат
                Condition = "Good"
            }
        };
        context.MediaItems.AddRange(mediaItems);
        context.SaveChanges();

        Console.WriteLine($"База данных заполнена начальными данными. Добавлено {mediaTypes.Count} типов носителей, {suppliers.Count} поставщиков, {movies.Count} фильмов и {mediaItems.Count} медиа-носителей.");
    }
    else
    {
        Console.WriteLine("База данных уже содержит данные. Пропускаем заполнение.");
    }
}