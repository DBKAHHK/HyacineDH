using EggLink.DanhengServer.GameServer.Server.Packet.Send.Marble;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Marble;

[Opcode(CmdIds.MarbleGetDataCsReq)]
public class HandlerMarbleGetDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketMarbleGetDataScRsp());
    }
}