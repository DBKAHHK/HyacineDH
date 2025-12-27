using EggLink.DanhengServer.GameServer.Server.Packet.Send.Marble;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Marble;

[Opcode(CmdIds.MarbleUpdateShownSealCsReq)]
public class HandlerMarbleUpdateShownSealCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = MarbleUpdateShownSealCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketMarbleUpdateShownSealScRsp(req.UpdateSealList));
    }
}