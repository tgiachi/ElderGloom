namespace ElderGloom.Network.Types;

public enum PacketType
{
    PlayerConnect,
    PlayerDisconnect,

    WorldState,
    PlayerPosition,
    PlayerAction,

    ChatMessage,

    Ping,
    Heartbeat
}
