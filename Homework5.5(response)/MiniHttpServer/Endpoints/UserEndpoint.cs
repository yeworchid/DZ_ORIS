using MiniHttpServer.Framework.Core.Attributes;
using System.Data.SqlClient;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class UserEndpoint
    {
        [HttpGet]
        public string GetUsers()
    {
        string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=usersdb;Integrated Security=True";
 
        string sqlExpression = "SELECT * FROM Users";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            SqlDataReader reader = command.ExecuteReader();
    
            if(reader.HasRows) // если есть данные
            {
                // выводим названия столбцов
                Console.WriteLine("{0}\t{1}\t{2}", reader.GetName(0), reader.GetName(1), reader.GetName(2));
    
                while (reader.Read()) // построчно считываем данные
                {
                    object id = reader.GetValue(0);
                    object name = reader.GetValue(1);
                    object age = reader.GetValue(2);
    
                    Console.WriteLine("{0} \t{1} \t{2}", id, name, age);
                }
            }
            
            reader.Close();
        }
                
        Console.Read();
        return "OK";
        }
    }
}