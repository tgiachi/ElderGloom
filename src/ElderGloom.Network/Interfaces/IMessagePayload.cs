using ElderGloom.Network.Types;

namespace ElderGloom.Network.Interfaces;

public interface IMessagePayload
{
    MessageType MessageType { get; }
}
