using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using VideoRentalSystem.Models;
using VideoRentalSystem.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// ВАЖНО: Устанавливаем кодировку для консоли
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Добавляем поддержку System.Web для UrlDecode
builder.Services.AddHttpContextAccessor();

// Подключение базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Video Rental API v1");
        c.RoutePrefix = "api-docs";
    });

    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Middleware для отладки запросов
app.Use(async (context, next) =>
{
    Console.WriteLine($"\n=== ВХОДЯЩИЙ ЗАПРОС ===");
    Console.WriteLine($"Метод: {context.Request.Method}");
    Console.WriteLine($"Путь: {context.Request.Path}");
    Console.WriteLine($"QueryString: {context.Request.QueryString}");

    // Логируем все параметры
    if (context.Request.Query.Any())
    {
        Console.WriteLine($"Параметры Query:");
        foreach (var key in context.Request.Query.Keys)
        {
            var value = context.Request.Query[key];
            try
            {
                var decodedValue = System.Web.HttpUtility.UrlDecode(value, System.Text.Encoding.UTF8);
                Console.WriteLine($"  {key}: '{value}' -> декодированное: '{decodedValue}'");
            }
            catch
            {
                Console.WriteLine($"  {key}: '{value}'");
            }
        }
    }

    await next();

    Console.WriteLine($"=== КОНЕЦ ЗАПРОСА ===\n");
});

// Маршрутизация
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

// Инициализация базы
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        SeedData(dbContext);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка при инициализации базы: {ex.Message}");
}

app.Run();

// Метод для заполнения начальными данными
static void SeedData(ApplicationDbContext context)
{
    // ВАЖНО: Сначала убедимся, что все таблицы созданы
    Console.WriteLine("Создание таблиц базы данных...");

    try
    {
        // Создаем все таблицы, если их нет
        context.Database.EnsureCreated();

        // Ждем немного
        System.Threading.Thread.Sleep(1000);

        Console.WriteLine("Таблицы созданы успешно");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при создании таблиц: {ex.Message}");
        return;
    }

    // Проверяем, есть ли уже данные
    if (!context.MediaTypes.Any())
    {
        Console.WriteLine("Заполнение базы данных начальными данными...");

        try
        {
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
                    Rating = 9.2m,
                    CoverImageUrl = "/images/movie1.jpg"
                },
                new Movie
                {
                    Title = "Побег из Шоушенка",
                    Description = "Два заключенных заводят дружбу в тюрьме.",
                    Year = 1994,
                    Genre = "Драма",
                    Director = "Фрэнк Дарабонт",
                    Duration = 142,
                    Rating = 9.3m,
                    CoverImageUrl = "/images/movie2.jpg"
                },
                new Movie
                {
                    Title = "Зеленая миля",
                    Description = "История тюремного надзирателя и его необычного заключенного.",
                    Year = 1999,
                    Genre = "Драма, Фэнтези",
                    Director = "Фрэнк Дарабонт",
                    Duration = 189,
                    Rating = 9.1m,
                    CoverImageUrl = "/images/movie3.jpg"
                },
                new Movie
                {
                    Title = "Форрест Гамп",
                    Description = "История человека с низким IQ, который стал свидетелем ключевых событий XX века.",
                    Year = 1994,
                    Genre = "Драма, Комедия",
                    Director = "Роберт Земекис",
                    Duration = 142,
                    Rating = 8.8m,
                    CoverImageUrl = "/images/movie4.jpg"
                },
                new Movie
                {
                    Title = "Назад в будущее",
                    Description = "Подросток попадает в прошлое на машине времени.",
                    Year = 1985,
                    Genre = "Фантастика, Комедия",
                    Director = "Роберт Земекис",
                    Duration = 116,
                    Rating = 8.5m,
                    CoverImageUrl = "/images/movie5.jpg"
                },
                new Movie
                {
                    Title = "Матрица",
                    Description = "Хакер Нео узнаёт, что его мир — виртуальная реальность.",
                    Year = 1999,
                    Genre = "Фантастика, Боевик",
                    Director = "Братья Вачовски",
                    Duration = 136,
                    Rating = 8.7m,
                    CoverImageUrl = "/images/movie6.jpg"
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
                    IsAvailable = true, // Изменено с false на true
                    Condition = "Good"
                },
                // Назад в будущее
                new MediaItem
                {
                    MovieId = 5,
                    MediaTypeId = 1, // VHS
                    SupplierId = 1,
                    Barcode = "VHS002",
                    PurchaseDate = DateTime.Now.AddYears(-2),
                    PurchasePrice = 7.99m,
                    IsAvailable = true,
                    Condition = "Fair"
                },
                // Матрица
                new MediaItem
                {
                    MovieId = 6,
                    MediaTypeId = 3, // Blu-Ray
                    SupplierId = 2,
                    Barcode = "BR003",
                    PurchaseDate = DateTime.Now.AddMonths(-1),
                    PurchasePrice = 24.99m,
                    IsAvailable = true,
                    Condition = "Excellent"
                },
                new MediaItem
                {
                    MovieId = 6,
                    MediaTypeId = 2, // DVD
                    SupplierId = 1,
                    Barcode = "DVD005",
                    PurchaseDate = DateTime.Now.AddMonths(-2),
                    PurchasePrice = 17.99m,
                    IsAvailable = false, // Выдана в прокат
                    Condition = "Good"
                }
            };
            context.MediaItems.AddRange(mediaItems);
            context.SaveChanges();

            Console.WriteLine($"База данных заполнена начальными данными.");
            Console.WriteLine($"Добавлено: {mediaTypes.Count} типов носителей");
            Console.WriteLine($"Добавлено: {suppliers.Count} поставщиков");
            Console.WriteLine($"Добавлено: {movies.Count} фильмов");
            Console.WriteLine($"Добавлено: {mediaItems.Count} медиа-носителей");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при заполнении данных: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    else
    {
        Console.WriteLine("База данных уже содержит данные. Пропускаем заполнение.");
    }
}