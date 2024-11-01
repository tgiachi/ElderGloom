using ElderGloom.Network.Types;

namespace ElderGloom.Network.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MessageTypeAttribute(MessageType messageType) : Attribute
{
    public MessageType MessageType { get; } = messageType;
}
