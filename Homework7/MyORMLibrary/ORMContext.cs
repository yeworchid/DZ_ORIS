using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Npgsql;
 
public class ORMContext
{
    private readonly string _connectionString;
 
    public ORMContext(string connectionString)
    {
        _connectionString = connectionString;
    }
 
    public T Create<T>(T entity, string tableName) where T : class
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            
            var columns = new List<string>();
            var valueParams = new List<string>();
            var command = new NpgsqlCommand();
            command.Connection = connection;
            
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    continue;
                
                columns.Add(property.Name);
                string paramName = $"@{property.Name}";
                valueParams.Add(paramName);
                
                object value = property.GetValue(entity);
                command.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
            }
            
            string sql = $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", valueParams)})";
            command.CommandText = sql;
            
            command.ExecuteNonQuery();
        }
        
        return entity;
    }
 
    public T ReadById<T>(int id, string tableName) where T : class, new()
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"SELECT * FROM {tableName} WHERE Id = @id";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
 
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return MapToObject<T>(reader);
                }
            }
        }
        return null;
    }
 
    public List<T> ReadAll<T>(string tableName) where T : class, new()
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"SELECT * FROM {tableName}";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
 
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                var results = new List<T>();
                while (reader.Read())
                {
                    results.Add(MapToObject<T>(reader));
                }
                return results;
            }
        }
    }
 
    public void Update<T>(int id, T entity, string tableName) where T : class
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            
            var setClauses = new List<string>();
            var command = new NpgsqlCommand();
            command.Connection = connection;
            
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    continue;
                
                string paramName = $"@{property.Name}";
                setClauses.Add($"{property.Name} = {paramName}");
                
                object value = property.GetValue(entity);
                command.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
            }
            
            string sql = $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE Id = @id";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@id", id);
            
            command.ExecuteNonQuery();
        }
    }
 
    public void Delete(int id, string tableName)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"DELETE FROM {tableName} WHERE Id = @id";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
 
            command.ExecuteNonQuery();
        }
    }

    // метод с фильтрацией
    public List<T> Read<T>(string tableName, string whereClause = null, Dictionary<string, object> parameters = null) where T : class, new()
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            
            string sql = $"SELECT * FROM {tableName}";
            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += $" WHERE {whereClause}";
            }
            
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }
 
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                var results = new List<T>();
                while (reader.Read())
                {
                    results.Add(MapToObject<T>(reader));
                }
                return results;
            }
        }
    }

    // FirstOrDefault с Expression
    public T FirstOrDefault<T>(string tableName, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            
            // парсим expression и генерируем SQL
            var (whereClause, parameters) = ExpressionParser.Parse(predicate);
            
            string sql = $"SELECT * FROM {tableName} WHERE {whereClause} LIMIT 1";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            
            // добавляем параметры
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
 
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return MapToObject<T>(reader);
                }
            }
        }
        return null;
    }

    // Where с Expression
    public List<T> Where<T>(string tableName, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            
            // парсим expression и генерируем SQL
            var (whereClause, parameters) = ExpressionParser.Parse(predicate);
            
            string sql = $"SELECT * FROM {tableName} WHERE {whereClause}";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            
            // добавляем параметры
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
 
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                var results = new List<T>();
                while (reader.Read())
                {
                    results.Add(MapToObject<T>(reader));
                }
                return results;
            }
        }
    }

    // маппинг из базы в объект
    private T MapToObject<T>(NpgsqlDataReader reader) where T : class, new()
    {
        T obj = new T();
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();
        
        foreach (PropertyInfo property in properties)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                
                if (property.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    object value = reader.GetValue(i);
                    
                    if (value != DBNull.Value)
                    {
                        property.SetValue(obj, value);
                    }
                    break;
                }
            }
        }
        
        return obj;
    }
}