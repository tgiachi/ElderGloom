using ElderGloom.Network.Packets;
using ElderGloom.Network.Types;
using MemoryPack;

namespace ElderGloom.Network.Interfaces;

[MemoryPackable]
[MemoryPackUnion(0, typeof(TestPacket))]
public partial interface IMessagePayload
{
    MessageType MessageType { get; }
}
