using System.Reflection;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class HandlerTests
    {
        // тест что Handler существует
        [TestMethod]
        public void Handler_ClassExists()
        {
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.Abstracts.Handler");
            
            Assert.IsNotNull(type);
        }

        // тест что Handler абстрактный
        [TestMethod]
        public void Handler_IsAbstract()
        {
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.Abstracts.Handler");
            
            Assert.IsTrue(type?.IsAbstract);
        }

        // тест что у Handler есть свойство Successor
        [TestMethod]
        public void Handler_HasSuccessorProperty()
        {
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.Abstracts.Handler");
            
            var property = type?.GetProperty("Successor");
            Assert.IsNotNull(property);
        }

        // тест что у Handler есть метод HandleRequest
        [TestMethod]
        public void Handler_HasHandleRequestMethod()
        {
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.Abstracts.Handler");
            
            var method = type?.GetMethod("HandleRequest");
            Assert.IsNotNull(method);
        }
    }
}
