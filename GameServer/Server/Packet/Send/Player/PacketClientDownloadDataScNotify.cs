using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using Google.Protobuf;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;

public class PacketClientDownloadDataScNotify : BasePacket
{
    public PacketClientDownloadDataScNotify(uint version = 1) : base(CmdIds.ClientDownloadDataScNotify)
    {
        var proto = new ClientDownloadDataScNotify
        {
            DownloadData = new ClientDownloadData
            {
                Version = version,
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Data = ByteString.Empty
            }
        };

        SetData(proto);
    }
}

