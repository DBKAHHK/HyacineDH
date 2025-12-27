using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;

public class PacketChangePropTimelineInfoScRsp : BasePacket
{
    public PacketChangePropTimelineInfoScRsp(uint entityId) : base(CmdIds.ChangePropTimelineInfoScRsp)
    {
        var proto = new ChangePropTimelineInfoScRsp
        {
            PropEntityId = entityId
        };

        SetData(proto);
    }
}