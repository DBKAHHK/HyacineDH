using EggLink.DanhengServer.GameServer.Server.Packet.Send.Match;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Match;

[Opcode(CmdIds.GetCrossInfoCsReq)]
public class HandlerGetCrossInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetCrossInfoScRsp());
    }
}