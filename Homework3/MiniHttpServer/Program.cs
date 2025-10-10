using System.Text.Json;
using MiniHttpServer.Shared;
using MiniHttpServer.Services;

var server = HttpServer.Instance;
await server.StartAsync();