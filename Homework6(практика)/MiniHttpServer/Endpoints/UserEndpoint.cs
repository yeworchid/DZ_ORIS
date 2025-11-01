using Npgsql;
using MiniHttpServer.Framework.Settings;
using MiniHttpServer.Framework.Core.HttpResponse;
using MiniHttpServer.Framework.Core.Attributes;

[Endpoint]
internal class UserEndpoint : BaseEndpoint
{
    [HttpGet]
    public async Task<IResponseResult> GetUsers()
    {
        try
        {
            string connectionString = Singleton.GetInstance().Settings.ConnectionString;
     
            string sqlExpression = "SELECT * FROM users";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var command = new NpgsqlCommand(sqlExpression, connection);
                var reader = await command.ExecuteReaderAsync();
        
                var users = new List<object>();
                
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new {
                            id = reader.GetValue(0),
                            name = reader.GetValue(1),
                            age = reader.GetValue(2)
                        });
                    }
                }
                reader.Close();
                
                return Json(users);
            }
        }
        catch (Exception ex)
        {
            return Json(new { error = "ошибка подключения к бд", message = ex.Message });
            // TODO: страница 500 и редирект
        }
    }
}