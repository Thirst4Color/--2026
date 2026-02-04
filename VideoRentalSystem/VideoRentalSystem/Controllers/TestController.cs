using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace VideoRentalSystem.Controllers
{
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult EncodingTest(string search = "")
        {
            Console.WriteLine("\n=== ТЕСТ КОДИРОВКИ ===");
            Console.WriteLine($"QueryString: {HttpContext.Request.QueryString}");

            // Все способы получить параметр
            Console.WriteLine($"\n1. Параметр search: '{search}'");
            Console.WriteLine($"2. Request.Query['search']: '{HttpContext.Request.Query["search"]}'");
            Console.WriteLine($"3. Request.QueryString: {HttpContext.Request.QueryString}");

            // Байтовое представление
            if (!string.IsNullOrEmpty(search))
            {
                Console.WriteLine($"\nБайты параметра search:");
                var bytes = Encoding.Default.GetBytes(search);
                Console.WriteLine($"Default encoding: {BitConverter.ToString(bytes)}");

                bytes = Encoding.UTF8.GetBytes(search);
                Console.WriteLine($"UTF-8 encoding: {BitConverter.ToString(bytes)}");

                bytes = Encoding.GetEncoding(1251).GetBytes(search);
                Console.WriteLine($"Windows-1251 encoding: {BitConverter.ToString(bytes)}");
            }

            // Пробуем разные способы декодирования
            var rawQuery = HttpContext.Request.Query["search"].ToString();
            if (!string.IsNullOrEmpty(rawQuery))
            {
                Console.WriteLine($"\nДекодирование rawQuery: '{rawQuery}'");

                // Способ 1: Uri.UnescapeDataString
                try
                {
                    var decoded1 = Uri.UnescapeDataString(rawQuery);
                    Console.WriteLine($"Uri.UnescapeDataString: '{decoded1}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Uri.UnescapeDataString error: {ex.Message}");
                }

                // Способ 2: WebUtility.UrlDecode
                try
                {
                    var decoded2 = System.Net.WebUtility.UrlDecode(rawQuery);
                    Console.WriteLine($"WebUtility.UrlDecode: '{decoded2}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebUtility.UrlDecode error: {ex.Message}");
                }

                // Способ 3: Convert from UTF-8 bytes
                try
                {
                    var bytes = Convert.FromBase64String(rawQuery);
                    var decoded3 = Encoding.UTF8.GetString(bytes);
                    Console.WriteLine($"FromBase64String: '{decoded3}'");
                }
                catch
                {
                    // Игнорируем ошибку
                }
            }

            return Content($"<h1>Тест кодировки</h1><pre>Проверьте консоль сервера</pre>", "text/html");
        }

        [HttpGet]
        public IActionResult SearchTest()
        {
            return Content(@"
                <html>
                <body>
                    <h1>Тест поиска</h1>
                    <form action=""/Catalog"" method=""get"">
                        <input type=""text"" name=""search"" value=""Крестный"" />
                        <button type=""submit"">Отправить GET</button>
                    </form>
                    <br/>
                    <form action=""/Test/EncodingTest"" method=""get"">
                        <input type=""text"" name=""search"" value=""Крестный"" />
                        <button type=""submit"">Тест кодировки</button>
                    </form>
                    <br/>
                    <a href=""/Catalog?search=%D0%9A%D1%80%D0%B5%D1%81%D1%82%D0%BD%D1%8B%D0%B9"">Прямая ссылка с кодировкой</a>
                </body>
                </html>
            ", "text/html");
        }
    }
}