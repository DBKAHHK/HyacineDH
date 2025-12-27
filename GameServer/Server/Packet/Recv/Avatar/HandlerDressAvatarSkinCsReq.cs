using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.DressAvatarSkinCsReq)]
public class HandlerDressAvatarSkinCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = DressAvatarSkinCsReq.Parser.ParseFrom(data);
        await connection.Player!.ChangeAvatarSkin((int)req.AvatarId, (int)req.SkinId);
        await connection.SendPacket(CmdIds.DressAvatarSkinScRsp);
    }
}