namespace MyApp.XProtocol;

public class XProtocolEncryptor
{
    private static string Key { get; } = "2e985f930853919313c96d001cb5701f";

    public static byte[] Encrypt(byte[] data)
    {
        return RijndaelHandler.Encrypt(data, Key);
    }

    public static byte[] Decrypt(byte[] data)
    {
        return RijndaelHandler.Decrypt(data, Key);
    }
}
