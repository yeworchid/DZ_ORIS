using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Framework.Core.HttpResponse;
using MiniHttpServer.Services;
using MiniHttpServer.Framework.Settings;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class TravelEndpoint : BaseEndpoint
    {
        [HttpGet]
        public string LoginPage()
        {
            var settings = Singleton.GetInstance().Settings;
            Console.WriteLine($"отдаем страничку: Travel");
            return "/Travel/index.html";
        }

    //     // обрабатываем логин
    //     [HttpPost]
    //     public async Task<IResponseResult> Login()
    //     {            
    //         try
    //         {
    //             // читаем данные из POST запроса (используем Context из BaseEndpoint)
    //             string requestBody;
    //             using (var reader = new StreamReader(Context.Request.InputStream))
    //             {
    //                 requestBody = await reader.ReadToEndAsync();
    //             }
                
    //             var loginData = JsonSerializer.Deserialize<LoginRequest>(requestBody);
                
    //             // шлем на указанную почту из формы
    //             string emailMessage = $@"
    //             Попытка входа в систему:
    //             Время: {DateTime.Now}
    //             Email: {loginData.email}
    //             Пароль: {loginData.password}
    //             ";
                
    //             Console.WriteLine($"шлем уведомление на {loginData.email}...");
    //             await EmailService.SendEmailAsync(loginData.email, "Попытка входа в систему", emailMessage, "attachments/project.zip");
                
    //             Console.WriteLine("✅ логин прошел");
                
    //             // возвращаем страницу с результатом логина
    //             return Page("Templates/Pages/login.thtml", new 
    //             { 
    //                 success = true,
    //                 email = loginData.email,
    //                 message = "Вход выполнен успешно!"
    //             });
    //         }
    //         catch (Exception ex)
    //         {
    //             Console.WriteLine($"❌ что-то пошло не так с логином: {ex.Message}");
                
    //             // возвращаем страницу с ошибкой
    //             return Page("Templates/Pages/login.thtml", new 
    //             { 
    //                 success = false,
    //                 message = $"Ошибка: {ex.Message}"
    //             });
    //         }
    //     }

    //     // тестируем отправку письма
    //     [HttpPost("sendEmail")]
    //     public async Task SendEmail()
    //     {
    //         Console.WriteLine("=== тестим отправку письма ===");
    //         await EmailService.SendEmailAsync("kaleevbd@gmail.com", "Тестовое письмо", "Это тестовое письмо из системы", "");
    //     }
    }
    
    // // чтобы парсить данные из формы логина
    // public class LoginRequest
    // {
    //     public string email { get; set; }
    //     public string password { get; set; }
    // }
}
