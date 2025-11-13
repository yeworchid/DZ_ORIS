using System.Net;
using System.Reflection;
using MiniHttpServer.Framework.Core.HttpResponse;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class JsonResultTests
    {
        // тест что JsonResult создается с данными
        [TestMethod]
        public void JsonResult_Constructor_AcceptsData()
        {
            var data = new { Name = "Test", Value = 123 };
            
            // используем рефлексию чтобы создать JsonResult (он internal)
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.HttpResponse.JsonResult");
            
            Assert.IsNotNull(type);
            var instance = Activator.CreateInstance(type, data);
            Assert.IsNotNull(instance);
        }

        // тест что JsonResult реализует IResponseResult
        [TestMethod]
        public void JsonResult_ImplementsIResponseResult()
        {
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.HttpResponse.JsonResult");
            
            Assert.IsTrue(typeof(IResponseResult).IsAssignableFrom(type));
        }
    }
}
