using EggLink.DanhengServer.GameServer.Server.Packet.Send.Recommend;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Recommend;

[Opcode(CmdIds.RelicSmartWearUpdatePlanCsReq)]
public class HandlerRelicSmartWearUpdatePlanCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = RelicSmartWearUpdatePlanCsReq.Parser.ParseFrom(data);
        connection.Player!.AvatarManager!.UpdateRelicPlan(req.RelicPlan);
        await connection.SendPacket(new PacketRelicSmartWearUpdatePlanScRsp(req.RelicPlan));
    }
}