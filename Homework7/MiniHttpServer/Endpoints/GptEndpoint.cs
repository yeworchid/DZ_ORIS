using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Services;
using MiniHttpServer.Framework.Settings;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class GptEndpoint
    {
        // /gpt
        [HttpGet]
        public string ChatGPT()
        {
            var settings = Singleton.GetInstance().Settings;
            Console.WriteLine($"отдаем страничку: {settings.ChatGPTUri}/index.html");
            return settings.ChatGPTUri + "/index.html";
        }
    }
}