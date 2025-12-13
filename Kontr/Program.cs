using System;
using System.Threading;
using Kontr;

Console.WriteLine("Запуск приложения...");

var settingsManager = new SettingsManager();
settingsManager.LoadSettings();
var settings = settingsManager.GetSettings();

var dbManager = new DbManager(settings.connection_string);
var httpServer = new HttpServer(settingsManager);

var serverThread = new Thread(() => httpServer.Start());
serverThread.Start();

var random = new Random();

Console.WriteLine($"Интервал: {settings.processing_interval_seconds}\nмакс попыток: {settings.max_error_retries}");

while (true)
{
    Console.WriteLine("Обработка инвойсов");

    try
    {
        var invoices = dbManager.GetPendingInvoices();
        Console.WriteLine($"Найдено: {invoices.Count}");

        foreach (var invoice in invoices)
        {
            bool success = random.Next(100) < 30;

            if (success)
            {
                dbManager.UpdateInvoice(invoice.Id, "success", invoice.RetryCount);
                Console.WriteLine($"Invoice {invoice.Id}: SUCCESS");
            }
            else
            {
                int newRetry = invoice.RetryCount + 1;

                if (newRetry >= settings.max_error_retries)
                {
                    dbManager.UpdateInvoice(invoice.Id, "error", newRetry);
                    Console.WriteLine($"Invoice {invoice.Id}: ERROR");
                }
                else
                {
                    dbManager.UpdateInvoice(invoice.Id, "pending", newRetry);
                    Console.WriteLine($"Invoice {invoice.Id}: RETRY {newRetry}/{settings.max_error_retries}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Ошибка: " + ex.Message);
    }

    Thread.Sleep(settings.processing_interval_seconds * 1000);
}
