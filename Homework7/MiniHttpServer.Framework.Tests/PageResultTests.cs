using System.Reflection;
using MiniHttpServer.Framework.Core.HttpResponse;

namespace MiniHttpServer.Framework.Tests
{
    [TestClass]
    public class PageResultTests
    {
        // тест что PageResult создается с параметрами
        [TestMethod]
        public void PageResult_Constructor_AcceptsPathAndData()
        {
            var data = new { Title = "Test Page" };
            var path = "template.html";
            
            // используем рефлексию чтобы создать PageResult (он internal)
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.HttpResponse.PageResult");
            
            Assert.IsNotNull(type);
            var instance = Activator.CreateInstance(type, path, data);
            Assert.IsNotNull(instance);
        }

        // тест что PageResult реализует IResponseResult
        [TestMethod]
        public void PageResult_ImplementsIResponseResult()
        {
            var assembly = Assembly.Load("MiniHttpServer.Framework");
            var type = assembly.GetType("MiniHttpServer.Framework.Core.HttpResponse.PageResult");
            
            Assert.IsTrue(typeof(IResponseResult).IsAssignableFrom(type));
        }
    }
}
