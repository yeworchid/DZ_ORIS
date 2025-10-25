using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTemplateEngine.Interfaces;
using System;
using System.IO;

[TestClass]
public class MiniTemplateEngineTests
{
    private readonly IHtmlTemplateRenderer _renderer = new HtmlTemplateRenderer();

    [TestMethod]
    public void RenderFromString_Variable_ReplacesName()
    {
        var template = "<h1>${user.Name}</h1>";
        var data = new { Name = "John" };
        var result = _renderer.RenderFromString(template, data);
        Assert.AreEqual("<h1>John</h1>", result);
    }

    [TestMethod]
    public void RenderFromString_IfTrue_ShowsActive()
    {
        var template = "$if(user.IsActive)\n<p>Active</p>\n$else\n<p>Not Active</p>\n$endif";
        var data = new { IsActive = true };
        var result = _renderer.RenderFromString(template, data);
        Assert.AreEqual("<p>Active</p>", result);
    }

    [TestMethod]
    public void RenderFromString_IfFalse_ShowsNotActive()
    {
        var template = "$if(user.IsActive)\n<p>Active</p>\n$else\n<p>Not Active</p>\n$endif";
        var data = new { IsActive = false };
        var result = _renderer.RenderFromString(template, data);
        Assert.AreEqual("<p>Not Active</p>", result);
    }

    [TestMethod]
    public void RenderFromString_Foreach_RendersItems()
    {
        var template = "$foreach(var item in user.Items)\n<p>${item.Name}</p>\n$endfor";
        var data = new { Items = new[] { new { Name = "Apple" }, new { Name = "Banana" } } };
        var result = _renderer.RenderFromString(template, data);
        Assert.AreEqual("<p>Apple</p><p>Banana</p>", result);
    }

    [TestMethod]
    public void RenderFromString_EmptyTemplate_ReturnsEmpty()
    {
        var template = "";
        var data = new { Name = "John" };
        var result = _renderer.RenderFromString(template, data);
        Assert.AreEqual("", result);
    }

    /*[TestMethod]
    public void RenderFromFile_SimpleTemplate_ReplacesName()
    {
        var filePath = "test.html";
        File.WriteAllText(filePath, "<p>${user.Name}</p>");
        var data = new { Name = "John" };
        var result = _renderer.RenderFromFile(filePath, data);
        File.Delete(filePath);
        Assert.AreEqual("<p>John</p>", result);
    }*/

    /*[TestMethod]
    public void RenderToFile_SimpleTemplate_WritesFile()
    {
        var inputFile = "input.html";
        var outputFile = "output.html";
        File.WriteAllText(inputFile, "<p>${user.Name}</p>");
        var data = new { Name = "John" };
        _renderer.RenderToFile(inputFile, outputFile, data);
        var result = File.ReadAllText(outputFile);
        File.Delete(inputFile);
        File.Delete(outputFile);
        Assert.AreEqual("<p>John</p>", result);
    }*/

    [TestMethod]
    public void RenderFromString_MultipleVariables_ReplacesAll()
    {
        var template = "<p>${user.Name} is ${user.Age} years old</p>";
        var data = new { Name = "John", Age = 25 };
        var result = _renderer.RenderFromString(template, data);
        Assert.AreEqual("<p>John is 25 years old</p>", result);
    }

    [TestMethod]
    public void RenderFromString_ForeachEmptyList_RendersNothing()
    {
        var template = "$foreach(var item in user.Items)\n<p>${item.Name}</p>\n$endfor";
        var data = new { Items = new object[] { } };
        var result = _renderer.RenderFromString(template, data);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void RenderFromString_CombinedTemplate_RendersAll()
    {
        var template = "<p>${user.Name}</p>\n$if(user.IsActive)\n<p>Active</p>\n$endif\n$foreach(var item in user.Items)\n<p>${item.Name}</p>\n$endfor";
        var data = new { Name = "John", IsActive = true, Items = new[] { new { Name = "Item1" } } };
        var result = _renderer.RenderFromString(template, data);
        Assert.AreEqual("<p>John</p><p>Active</p><p>Item1</p>", result);
    }

    /*[TestMethod]
    [ExpectedException(typeof(FileNotFoundException))]
    public void RenderFromFile_FileMissing_ThrowsException()
    {
        _renderer.RenderFromFile("missing.html", new { Name = "John" });
    }*/

    [TestMethod]
    public void RenderFromString_UnknownVariable_KeepsPlaceholder()
    {
        var template = "<p>${user.Unknown}</p>";
        var data = new { Name = "John" };
        var result = _renderer.RenderFromString(template, data);
        Assert.AreEqual("<p>${user.Unknown}</p>", result);
    }
}