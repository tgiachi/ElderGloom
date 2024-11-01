using ElderGloom.Network.Types;

namespace ElderGloom.Network.Packet;

public class NetworkPacket
{
    public PacketType Type { get; set; }
    public MessageType MessageType { get; set; }
    public byte[] Payload { get; set; }


    public NetworkPacket()
    {
    }

    public NetworkPacket(PacketType type, MessageType messageType, byte[] payload = null)
    {
        Type = type;
        MessageType = messageType;
        Payload = payload;
    }
}
