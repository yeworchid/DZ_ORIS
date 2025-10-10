namespace MiniHttpServer.Shared;

public class MimeTypes
{
    public static readonly Dictionary<string, string> ByExtension = new()
    {
        ["html"] = "text/html",
        ["css"] = "text/css",
        ["js"] = "application/javascript",
        ["png"] = "image/png",
        ["jpg"] = "image/jpeg",
        ["jpeg"] = "image/jpeg",
        ["webp"] = "image/webp"
    };

    public static string GetMimeType(string filePath)
    {
        var fileExtension = filePath.Split('.').Last().ToLower();
        if (MimeTypes.ByExtension.ContainsKey(fileExtension))
        {
            return MimeTypes.ByExtension[fileExtension];
        }
        
        return null;
    }
}