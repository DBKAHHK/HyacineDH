using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.ChangePropTimelineInfoCsReq)]
public class HandlerChangePropTimelineInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ChangePropTimelineInfoCsReq.Parser.ParseFrom(data);

        await connection.Player!.SetPropTimeline((int)req.PropEntityId, req.TimelineInfo);
        await connection.SendPacket(new PacketChangePropTimelineInfoScRsp(req.PropEntityId));
    }
}