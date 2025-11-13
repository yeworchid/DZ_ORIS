using MiniHttpServer.Framework.Core.Abstracts;
using MiniHttpServer.Framework.Core.Handlers;
using MiniHttpServer.Framework.Settings;
using MiniHttpServer.Framework.Shared;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace MiniHttpServer.Framework.Server 
{
    public class HttpServer
    {
        private HttpListener _listener = new();
        private JsonEntity _config;
        private CancellationToken _token;

        public HttpServer(JsonEntity config) { _config = config; }

        public void Start(CancellationToken token)
        {
            _token = token;
            _listener = new HttpListener();
            string url = "http://" + _config.Domain + ":" + _config.Port + "/";
            _listener.Prefixes.Add(url);
            _listener.Start();
            Console.WriteLine("Сервер запущен! Проверяй в браузере: " + url);
            Receive();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        protected async void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening && !_token.IsCancellationRequested)
            {
                var context = _listener.EndGetContext(result);

                Handler staticFilesHandler = new StaticFilesHandler();
                Handler endpointsHandler = new EndpointsHandler();
                Handler notFoundHandler = new NotFoundHandler();
                
                staticFilesHandler.Successor = endpointsHandler;
                endpointsHandler.Successor = notFoundHandler;
                
                staticFilesHandler.HandleRequest(context);

                if (!_token.IsCancellationRequested)
                    Receive();
            }
        }
    }
}