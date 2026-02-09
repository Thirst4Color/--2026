using Microsoft.EntityFrameworkCore;
using VideoRentalSystem.Models;
using VideoRentalSystem.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Установка кодировки
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Добавляем сервисы
builder.Services.AddControllersWithViews();

// Сессии
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// База данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Конфигурация
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Сессии
app.UseSession();

// Маршруты
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

// Инициализация БД
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
        SeedData(db);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка инициализации БД: {ex.Message}");
}

app.Run();

// Упрощенная SeedData
static void SeedData(ApplicationDbContext context)
{
    Console.WriteLine("=== НАЧАЛО ЗАПОЛНЕНИЯ БАЗЫ ДАННЫХ ===");

    try
    {
        // Создаем все таблицы
        context.Database.EnsureCreated();
        Console.WriteLine("✅ Таблицы созданы");

        // 1. ТИПЫ НОСИТЕЛЕЙ
        if (!context.MediaTypes.Any())
        {
            Console.WriteLine("Добавляем типы носителей...");
            var mediaTypes = new List<MediaType>
            {
                new MediaType { Name = "VHS", DailyRentalPrice = 1.50m },
                new MediaType { Name = "DVD", DailyRentalPrice = 2.00m },
                new MediaType { Name = "Blu-Ray", DailyRentalPrice = 2.50m },
                new MediaType { Name = "HD-DVD", DailyRentalPrice = 2.20m }
            };
            context.MediaTypes.AddRange(mediaTypes);
            context.SaveChanges();
            Console.WriteLine($"✅ Добавлено {mediaTypes.Count} типов носителей");
        }
        else
        {
            Console.WriteLine("Типы носителей уже существуют");
        }

        // 2. ПОСТАВЩИКИ
        if (!context.Suppliers.Any())
        {
            Console.WriteLine("Добавляем поставщиков...");
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
            Console.WriteLine($"✅ Добавлено {suppliers.Count} поставщиков");
        }

        // 3. ФИЛЬМЫ
        if (!context.Movies.Any())
        {
            Console.WriteLine("Добавляем фильмы...");
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
            Console.WriteLine($"✅ Добавлено {movies.Count} фильмов");
        }

        // 4. МЕДИА-НОСИТЕЛИ
        if (!context.MediaItems.Any() && context.Movies.Any() && context.MediaTypes.Any() && context.Suppliers.Any())
        {
            Console.WriteLine("Добавляем медиа-носители...");

            // Получаем ID для связей
            var vhs = context.MediaTypes.First(mt => mt.Name == "VHS");
            var dvd = context.MediaTypes.First(mt => mt.Name == "DVD");
            var bluray = context.MediaTypes.First(mt => mt.Name == "Blu-Ray");
            var supplier1 = context.Suppliers.First();
            var supplier2 = context.Suppliers.Skip(1).First();

            var movies = context.Movies.ToList();

            var mediaItems = new List<MediaItem>();

            // Крестный отец
            var godfather = movies.First(m => m.Title.Contains("Крестный"));
            mediaItems.Add(new MediaItem
            {
                MovieId = godfather.MovieId,
                MediaTypeId = dvd.MediaTypeId,
                SupplierId = supplier1.SupplierId,
                Barcode = "DVD001",
                PurchaseDate = DateTime.Now.AddMonths(-6),
                PurchasePrice = 15.99m,
                IsAvailable = true,
                Condition = "Good"
            });

            mediaItems.Add(new MediaItem
            {
                MovieId = godfather.MovieId,
                MediaTypeId = bluray.MediaTypeId,
                SupplierId = supplier1.SupplierId,
                Barcode = "BR001",
                PurchaseDate = DateTime.Now.AddMonths(-3),
                PurchasePrice = 25.99m,
                IsAvailable = true,
                Condition = "Excellent"
            });

            // Побег из Шоушенка
            var shawshank = movies.First(m => m.Title.Contains("Побег"));
            mediaItems.Add(new MediaItem
            {
                MovieId = shawshank.MovieId,
                MediaTypeId = dvd.MediaTypeId,
                SupplierId = supplier2.SupplierId,
                Barcode = "DVD002",
                PurchaseDate = DateTime.Now.AddMonths(-5),
                PurchasePrice = 12.99m,
                IsAvailable = true,
                Condition = "Good"
            });

            // Зеленая миля
            var greenMile = movies.First(m => m.Title.Contains("Зеленая"));
            mediaItems.Add(new MediaItem
            {
                MovieId = greenMile.MovieId,
                MediaTypeId = dvd.MediaTypeId,
                SupplierId = supplier1.SupplierId,
                Barcode = "DVD003",
                PurchaseDate = DateTime.Now.AddMonths(-4),
                PurchasePrice = 14.99m,
                IsAvailable = true,
                Condition = "Excellent"
            });

            // Форрест Гамп
            var forrest = movies.First(m => m.Title.Contains("Форрест"));
            mediaItems.Add(new MediaItem
            {
                MovieId = forrest.MovieId,
                MediaTypeId = bluray.MediaTypeId,
                SupplierId = supplier2.SupplierId,
                Barcode = "BR002",
                PurchaseDate = DateTime.Now.AddMonths(-2),
                PurchasePrice = 22.99m,
                IsAvailable = true,
                Condition = "Excellent"
            });

            // Матрица
            var matrix = movies.First(m => m.Title.Contains("Матрица"));
            mediaItems.Add(new MediaItem
            {
                MovieId = matrix.MovieId,
                MediaTypeId = bluray.MediaTypeId,
                SupplierId = supplier2.SupplierId,
                Barcode = "BR003",
                PurchaseDate = DateTime.Now.AddMonths(-1),
                PurchasePrice = 24.99m,
                IsAvailable = true,
                Condition = "Excellent"
            });

            context.MediaItems.AddRange(mediaItems);
            context.SaveChanges();
            Console.WriteLine($"✅ Добавлено {mediaItems.Count} медиа-носителей");
        }

        Console.WriteLine("=== БАЗА ДАННЫХ ГОТОВА ===");
        Console.WriteLine($"Фильмы: {context.Movies.Count()}");
        Console.WriteLine($"Медиа-носители: {context.MediaItems.Count()}");
        Console.WriteLine($"Доступные носители: {context.MediaItems.Count(mi => mi.IsAvailable)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при заполнении базы данных: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
    }
}