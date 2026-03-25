using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace TeamCardSignal.Hooks;

public struct TeamCardSignalMessage : INetMessage, IPacketSerializable
{
    public string Payload;

    public bool ShouldBroadcast => true;

    public NetTransferMode Mode => NetTransferMode.Reliable;

    public LogLevel LogLevel => LogLevel.Debug;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteString(Payload ?? string.Empty);
    }

    public void Deserialize(PacketReader reader)
    {
        Payload = reader.ReadString();
    }

    public override string ToString()
    {
        return $"TeamCardSignalMessage({Payload})";
    }
}
