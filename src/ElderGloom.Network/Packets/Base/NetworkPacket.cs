using ElderGloom.Network.Types;
using LiteNetLib.Utils;

namespace ElderGloom.Network.Packets.Base;

public class NetworkPacket : INetSerializable
{
    public DateTime Timestamp { get; set; }
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
        Timestamp = DateTime.UtcNow;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)Type);
        writer.Put((byte)MessageType);
        writer.Put(Payload);
        writer.Put(Timestamp.ToBinary());
    }

    public void Deserialize(NetDataReader reader)
    {
        Type = (PacketType)reader.GetByte();
        MessageType = (MessageType)reader.GetByte();
        Payload = reader.GetBytesWithLength();
        Timestamp = DateTime.FromBinary(reader.GetLong());
    }
}
