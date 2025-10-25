using MiniHttpServer.Core.Abstracts;
using MiniHttpServer.Core.Attributes;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace MiniHttpServer.Core.Handlers
{
    internal class EndpointsHandler : Handler
    {
        public override async void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var pathSegments = request.Url?.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            if (pathSegments == null || pathSegments.Length == 0)
            {
                if (Successor != null)
                    Successor.HandleRequest(context);
                return;
            }

            var endpointName = pathSegments[0];

            var assembly = Assembly.GetExecutingAssembly();
            var endpont = assembly.GetTypes()
                                   .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
                                   .FirstOrDefault(end => IsCheckedNameEndpoint(end.Name, endpointName));

            if (endpont == null) 
            {
                if (Successor != null)
                    Successor.HandleRequest(context);
                return;
            }

            var method = endpont.GetMethods().Where(t => t.GetCustomAttributes(true)
                        .Any(attr => attr.GetType().Name.Equals($"Http{context.Request.HttpMethod}", 
                                                                StringComparison.OrdinalIgnoreCase)))
                        .FirstOrDefault();

            if (method == null) 
            {
                if (Successor != null)
                    Successor.HandleRequest(context);
                return;
            }

            object ret;
            var parameters = method.GetParameters();
            if (parameters.Length > 0 && parameters[0].ParameterType == typeof(HttpListenerContext))
            {
                ret = method.Invoke(Activator.CreateInstance(endpont), new object[] { context });
            }
            else
            {
                ret = method.Invoke(Activator.CreateInstance(endpont), null);
            }
            
            // Если метод асинхронный, ждем результат
            if (ret is Task task)
            {
                await task;
                if (task.GetType().IsGenericType)
                {
                    ret = ((dynamic)task).Result;
                }
                else
                {
                    ret = null;
                }
            }
            
            var response = context.Response;
            
            if (ret != null && ret is string result)
            {
                // Если возвращается имя файла, загружаем его
                byte[]? buffer = null;
                if (result.EndsWith(".html") || result.EndsWith(".css") || result.EndsWith(".js"))
                {
                    buffer = MiniHttpServer.Shared.GetResponseBytes.Invoke(result);
                    response.ContentType = MiniHttpServer.Shared.ContentType.GetContentType(result);
                }
                else
                {
                    // Иначе возвращаем как текст
                    buffer = System.Text.Encoding.UTF8.GetBytes(result);
                    response.ContentType = "text/plain";
                }

                if (buffer != null)
                {
                    response.ContentLength64 = buffer.Length;
                    using var output = response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    await output.FlushAsync();
                    
                    Console.WriteLine($"Эндпоинт обработан: {request.Url.AbsolutePath} - Status: {response.StatusCode}");
                }
            }
            else
            {
                // Если метод ничего не возвращает, отправляем пустой ответ
                response.StatusCode = 200;
                response.Close();
                Console.WriteLine($"Эндпоинт обработан: {request.Url.AbsolutePath} - Status: {response.StatusCode}");
            }
        }

        private bool IsCheckedNameEndpoint(string endpointName, string className) =>
            endpointName.Equals(className, StringComparison.OrdinalIgnoreCase) ||
            endpointName.Equals($"{className}Endpoint", StringComparison.OrdinalIgnoreCase);


    }
}
