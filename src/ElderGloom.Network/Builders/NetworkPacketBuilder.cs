using System.Reflection;
using ElderGloom.Network.Attributes;

using ElderGloom.Network.Packets.Base;
using ElderGloom.Network.Types;
using ElderGloom.Network.Utils;
using MemoryPack;
using Serilog;

namespace ElderGloom.Network.Builders;

public class NetworkPacketBuilder
{
    private MessageType _messageType;

    private object _payload;

    private bool _isCompressed;
    private bool _isEncrypted;

    private byte[] _encryptionKey;

    public static NetworkPacketBuilder Create() => new();


    public NetworkPacketBuilder WithEncryptionKey(byte[] encryptionKey)
    {
        _encryptionKey = encryptionKey;
        _isEncrypted = true;
        return this;
    }

    public NetworkPacketBuilder WithCompression()
    {
        _isCompressed = true;
        return this;
    }


    public NetworkPacketBuilder WithPayload<TEntity>(TEntity payload) where TEntity : class
    {
        _payload = payload;
        _messageType = payload.GetType().GetCustomAttribute<MessageTypeAttribute>().MessageType;

        return this;
    }

    public NetworkPacket Build()
    {
        var logger = Log.Logger.ForContext<NetworkPacketBuilder>();

        var packetType = PacketType.None;

        var payload = MemoryPackSerializer.Serialize(_payload);

        logger.Debug("Building packet with type {MessageType} and payload {Payload}", _messageType, _payload);

        logger.Debug("Total bytes {Bytes}", payload.Length);


        if (_isEncrypted)
        {
            packetType |= PacketType.Encrypted;

            payload = CryptoHelper.Encrypt(payload, _encryptionKey);

            logger.Debug("Encrypted bytes {Bytes}", payload.Length);
        }

        if (_isCompressed)
        {
            packetType |= PacketType.Compressed;

            payload = CompressionHelper.Compress(payload);

            logger.Debug("Compressed bytes {Bytes}", payload.Length);
        }

        logger.Debug("Final bytes {Bytes}", payload.Length);

        return new NetworkPacket(packetType, _messageType, payload);
    }
}
