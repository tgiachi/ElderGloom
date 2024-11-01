using System.Text;
using ElderGloom.Core.Extensions;
using ElderGloom.Network.Interfaces;
using ElderGloom.Network.Packet;
using ElderGloom.Network.Types;
using ElderGloom.Network.Utils;

namespace ElderGloom.Network.Builders;

public class NetworkPacketBuilder
{
    private MessageType _messageType;

    private IMessagePayload _payload;

    public bool _isCompressed;
    public bool _isEncrypted;

    public byte[] _encryptionKey;


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


    public NetworkPacketBuilder WithMessageType(MessageType messageType)
    {
        _messageType = messageType;
        return this;
    }

    public NetworkPacketBuilder WithPayload<TEntity>(TEntity payload) where TEntity : IMessagePayload
    {
        _payload = payload;
        return this;
    }

    public NetworkPacket Build()
    {
        var packetType = PacketType.None;

        var payload = Encoding.UTF8.GetBytes(_payload.ToJson());


        if (_isEncrypted)
        {
            packetType |= PacketType.Encrypted;

            payload = CryptoHelper.Encrypt(payload, _encryptionKey);
        }

        if (_isCompressed)
        {
            packetType |= PacketType.Compressed;

            payload = CompressionHelper.Compress(payload);
        }

        return new NetworkPacket(packetType, _messageType, payload);
    }
}
