using EggLink.DanhengServer.GameServer.Server.Packet.Send.Recommend;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Recommend;

[Opcode(CmdIds.GetBigDataRecommendCsReq)]
public class HandlerGetBigDataRecommendCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetBigDataRecommendCsReq.Parser.ParseFrom(data);
        await connection.SendPacket(new PacketGetBigDataRecommendScRsp(req.EquipAvatar, req.BigDataRecommendType));
    }
}