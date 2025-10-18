using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using MiniHttpServer.Core.Attributes;
using MiniHttpServer.Services;
using MiniHttpServer.Settings;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class BonxEndpoint
    {
        // /bonx
        [HttpGet]
        public string Bonx()
        {
            var settings = Singleton.GetInstance().Settings;
            Console.WriteLine($"отдаем страничку: bonx/index.html");
            return "bonx/index.html";
        }
    }
}