using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using MiniHttpServer.Core.Attributes;
using MiniHttpServer.Services;
using MiniHttpServer.Settings;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class AuthEndpoint
    {
        // показываем страницу логина
        [HttpGet]
        public string LoginPage()
        {
            var settings = Singleton.GetInstance().Settings;
            Console.WriteLine($"отдаем страничку: {settings.LoginUri}/login.html");
            return settings.LoginUri + "/login.html";
        }

        // обрабатываем логин
        [HttpPost]
        public async Task<string> Login(HttpListenerContext context)
        {            
            try
            {
                // читаем данные из POST запроса
                string requestBody;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                
                var loginData = JsonSerializer.Deserialize<LoginRequest>(requestBody);
                
                // шлем на указанную почту из формы
                string emailMessage = $@"
                Попытка входа в систему:
                Время: {DateTime.Now}
                Email: {loginData.email}
                Пароль: {loginData.password}
                ";
                
                Console.WriteLine($"шлем уведомление на {loginData.email}...");
                await EmailService.SendEmailAsync(loginData.email, "Попытка входа в систему", emailMessage);
                
                Console.WriteLine("✅ все ок, логин прошел");
                return "OK";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ что-то пошло не так с логином: {ex.Message}");
                throw;
            }
        }

        // тестируем отправку письма
        [HttpPost("sendEmail")]
        public async Task SendEmail()
        {
            Console.WriteLine("=== тестим отправку письма ===");
            await EmailService.SendEmailAsync("kaleevbd@gmail.com", "Тестовое письмо", "Это тестовое письмо из системы");
        }
    }
    
    // чтобы парсить данные из формы логина
    public class LoginRequest
    {
        public string email { get; set; }
        public string password { get; set; }
    }
}
