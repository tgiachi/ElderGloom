namespace ElderGloom.Network.Types;

[Flags]
public enum PacketType : byte
{
    None,
    Encrypted,
    Compressed,
}
