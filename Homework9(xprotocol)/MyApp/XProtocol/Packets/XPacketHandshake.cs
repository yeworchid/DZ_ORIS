namespace MyApp.XProtocol.Packets;

public class XPacketHandshake
{
    [XField(1)]
    public int MagicHandshakeNumber;
}
