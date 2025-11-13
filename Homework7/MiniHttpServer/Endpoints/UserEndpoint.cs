using Npgsql;
using MiniHttpServer.Framework.Settings;
using MiniHttpServer.Framework.Core.HttpResponse;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Models;
using MyORMLibrary;

[Endpoint]
internal class UserEndpoint : BaseEndpoint
{
    [HttpGet]
    public async Task<IResponseResult> GetUsers()
    {
        var settings = Singleton.GetInstance().Settings;
        var orm = new ORMContext(settings.ConnectionString);
        var users = orm.ReadAll<User>("users");
        return Json(users);
    }
}