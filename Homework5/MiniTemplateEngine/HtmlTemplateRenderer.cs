using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MiniTemplateEngine.Interfaces;
using MiniTemplateEngine.Utils;

public class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        // обработка foreach
        // htmlTemplate = ProcessForeach(htmlTemplate, dataModel);
        // // обработка if
        // htmlTemplate = ProcessIfElse(htmlTemplate, dataModel);
        // простая подстановка
        return Regex.Replace(htmlTemplate, @"\$\{([^}]+)\}", match =>
        {
            var key = match.Groups[1].Value;
            return Resolve(key, dataModel);
        });
    }

    // private string ProcessIfElse(string htmlTemplate, object dataModel)
    // {

    //     throw new NotImplementedException();
    // }

    // private string ProcessForeach(string htmlTemplate, object dataModel)
    // {
    //     var stack = new Stack<BlockContext>();
    //     var output = new StringBuilder();
    //     var lines = htmlTemplate.Split('\n');

    //     foreach (var line in lines)
    //     {
    //         if (line.Trim().StartsWith("$if"))
    //         {
    //             stack.Push(new BlockContext { Type = BlockType.If, ConditionResult = true, Body = new List<string>() });
    //         }
    //         if (line.Trim().StartsWith("$else"))
    //         {
    //             stack.Push(new BlockContext { Type = BlockType.If, ConditionResult = true, Body = new List<string>() });
    //         }
    //         if (line.Trim().StartsWith("$endif"))
    //         {
    //             stack.Push(new BlockContext { Type = BlockType.If, ConditionResult = true, Body = new List<string>() });
    //         }
    //         if (line.Trim().StartsWith("$foreach"))
    //         {
    //             stack.Push(new BlockContext { Type = BlockType.For, ConditionResult = true, Body = new List<string>() });
    //         }
    //         if (line.Trim().StartsWith("$endfor"))
    //         {
    //             stack.Push(new BlockContext { Type = BlockType.If, ConditionResult = true, Body = new List<string>() });
    //         }

    //         // Обычная строка
    //         if (stack.Count == 0)
    //             output.AppendLine(line);
    //         else
    //             stack.Peek().Body.Add(line);
    //     }

    //     throw new NotImplementedException();
    // }

    // private static bool CheckCondition(string expression, object model)
    // {
    //     expression = expression.Trim();

    //     // поддержка отрицания
    //     bool negate = false;
    //     if (expression.StartsWith("!"))
    //     {
    //         negate = true;
    //         expression = expression.Substring(1).Trim();
    //     }

    //     // поддержка простейших сравнений
    //     string[] ops = { "==", "!=", ">=", "<=", ">", "<" };
    //     foreach (var op in ops)
    //     {
    //         var parts = expression.Split(op);
    //         if (parts.Length == 2)
    //         {
    //             var left = Resolve(parts[0].Trim(), model);
    //             var right = parts[1].Trim().Trim('"', '\'');

    //             int comparison = 0;
    //             if (left is IComparable cmp)
    //                 comparison = cmp.CompareTo(Convert.ChangeType(right, left.GetType()));

    //             bool result = op switch
    //             {
    //                 "==" => left?.ToString() == right,
    //                 "!=" => left?.ToString() != right,
    //                 ">" => comparison > 0,
    //                 "<" => comparison < 0,
    //                 ">=" => comparison >= 0,
    //                 "<=" => comparison <= 0,
    //                 _ => false
    //             };

    //             return negate ? !result : result;
    //         }
    //     }

    //     // если нет оператора - просто проверяем логическое свойство
    //     var value = Resolve(expression, model);
    //     if (value is string s && bool.TryParse(s, out var sb))
    //         return negate ? !sb : sb;


    //     // все остальное
    //     return false;
    // }

    public string RenderFromFile(string filePath, object dataModel)
    {
        throw new NotImplementedException();
    }

    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        throw new NotImplementedException();
    }

    private static string Resolve(string path, object model)
    {
        object? current = model;

        foreach (var part in path.Split('.'))
        {
            if (string.IsNullOrWhiteSpace(part) || current == null)
                return "";

            var prop = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return "";

            current = prop.GetValue(current);
        }

        return current?.ToString() ?? "";
    }
}