using ElderGloom.Network.Builders;
using ElderGloom.Network.Packets;
using ElderGloom.Network.Types;

namespace ElderGloom.Core.Tests;

public class PacketTests
{
    [Fact]
    public void PacketBuilderCreatePacket()
    {
        var packet = NetworkPacketBuilder.Create()
            .WithCompression()
            .WithPayload(new TestPacket())
            .Build();


        Assert.NotNull(packet.Payload);
    }
}
