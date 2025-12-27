using EggLink.DanhengServer.GameServer.Server.Packet.Send.Recommend;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Recommend;

[Opcode(CmdIds.GetBigDataAllRecommendCsReq)]
public class HandlerGetBigDataAllRecommendCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetBigDataAllRecommendCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketGetBigDataAllRecommendScRsp(req.BigDataRecommendType));
    }
}