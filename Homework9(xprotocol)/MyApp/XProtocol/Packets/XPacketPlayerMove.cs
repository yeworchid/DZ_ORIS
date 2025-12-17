namespace MyApp.XProtocol.Packets;

public class XPacketPlayerMove
{
    [XField(1)]
    public int X;
    
    [XField(2)]
    public int Y;
    
    [XField(3)]
    public int R;
}
