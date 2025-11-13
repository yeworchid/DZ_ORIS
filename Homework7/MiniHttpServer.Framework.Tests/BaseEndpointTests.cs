using System.Reflection;
using MiniHttpServer.Framework.Core.HttpResponse;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class BaseEndpointTests
    {
        // тест что BaseEndpoint существует
        [TestMethod]
        public void BaseEndpoint_ClassExists()
        {
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.HttpResponse.BaseEndpoint");
            
            Assert.IsNotNull(type);
        }

        // тест что у BaseEndpoint есть метод Json
        [TestMethod]
        public void BaseEndpoint_HasJsonMethod()
        {
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.HttpResponse.BaseEndpoint");
            
            var method = type?.GetMethod("Json", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);
        }

        // тест что у BaseEndpoint есть метод Page
        [TestMethod]
        public void BaseEndpoint_HasPageMethod()
        {
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.HttpResponse.BaseEndpoint");
            
            var method = type?.GetMethod("Page", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);
        }
    }
}
