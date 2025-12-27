using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.RelicReforgeCsReq)]
public class HandlerRelicReforgeCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = RelicReforgeCsReq.Parser.ParseFrom(data);
        await connection.Player!.AvatarManager!.ReforgeRelic((int)req.RelicUniqueId);
        await connection.SendPacket(CmdIds.RelicReforgeScRsp);
    }
}