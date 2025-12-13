using Npgsql;
using System;
using System.Collections.Generic;

public class Invoice
{
    public int Id { get; set; }
    public string BankName { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}

public class DbManager
{
    private string _connectionString;

    public DbManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Invoice> GetPendingInvoices()
    {
        var invoices = new List<Invoice>();

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new NpgsqlCommand("SELECT Id, BankName, Amount, Status, UpdatedAt, RetryCount, LastAttemptAt FROM Invoices WHERE Status = 'pending'", conn);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var invoice = new Invoice
                {
                    Id = reader.GetInt32(0),
                    BankName = reader.GetString(1),
                    Amount = reader.GetDecimal(2),
                    Status = reader.GetString(3),
                    UpdatedAt = reader.GetDateTime(4),
                    RetryCount = reader.GetInt32(5),
                    LastAttemptAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
                };
                invoices.Add(invoice);
            }
        }

        return invoices;
    }

    public void UpdateInvoice(int id, string status, int retryCount)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new NpgsqlCommand(@"
                UPDATE Invoices 
                SET Status = @status, 
                    UpdatedAt = NOW(), 
                    RetryCount = @retryCount, 
                    LastAttemptAt = NOW() 
                WHERE Id = @id", conn);

            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("retryCount", retryCount);

            cmd.ExecuteNonQuery();
        }
    }
}
