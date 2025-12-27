using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.ClientObjUploadCsReq)]
public class HandlerClientObjUploadCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ClientObjUploadCsReq.Parser.ParseFrom(data);
        await connection.SendPacket(new PacketClientObjUploadScRsp(req.ModifierContentType));
    }
}

