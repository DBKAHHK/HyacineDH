using EggLink.DanhengServer.GameServer.Server.Packet.Send.Marble;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Marble;

[Opcode(CmdIds.MarbleLevelFinishCsReq)]
public class HandlerMarbleLevelFinishCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = MarbleLevelFinishCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketMarbleLevelFinishScRsp(req.MarbleLevelId));
    }
}