using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.TakeOffAvatarSkinCsReq)]
public class HandlerTakeOffAvatarSkinCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = TakeOffAvatarSkinCsReq.Parser.ParseFrom(data);
        await connection.Player!.ChangeAvatarSkin((int)req.AvatarId, 0);
        await connection.SendPacket(CmdIds.TakeOffAvatarSkinScRsp);
    }
}