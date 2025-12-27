using EggLink.DanhengServer.GameServer.Server.Packet.Send.MarkChest;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.MarkChest;

[Opcode(CmdIds.GetMarkChestCsReq)]
public class HandlerGetMarkChestCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetMarkChestScRsp(connection.Player!));
    }
}