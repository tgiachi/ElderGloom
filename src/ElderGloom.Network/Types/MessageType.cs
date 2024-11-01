namespace ElderGloom.Network.Types;

public enum MessageType : byte
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
