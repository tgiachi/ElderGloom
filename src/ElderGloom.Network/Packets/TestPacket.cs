using ElderGloom.Network.Attributes;

using ElderGloom.Network.Types;
using MemoryPack;

namespace ElderGloom.Network.Packets;

[MemoryPackable]
[MessageType(MessageType.Ping)]
public partial class TestPacket
{
    public DateTime Timestamp { get; set; }
}
