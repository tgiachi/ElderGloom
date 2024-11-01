using ElderGloom.Network.Interfaces;
using ElderGloom.Network.Types;
using MemoryPack;

namespace ElderGloom.Network.Packets;

[MemoryPackable]
public partial class TestPacket : IMessagePayload
{
    public MessageType MessageType => MessageType.Ping;

    public DateTime Timestamp { get; set; }
}
