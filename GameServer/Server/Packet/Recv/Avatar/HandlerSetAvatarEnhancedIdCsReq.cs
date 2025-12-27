using EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerSync;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.SetAvatarEnhancedIdCsReq)]
public class HandlerSetAvatarEnhancedIdCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetAvatarEnhancedIdCsReq.Parser.ParseFrom(data);

        // get target avatar
        var avatar = connection.Player!.AvatarManager!.GetFormalAvatar((int)req.AvatarId);
        var path = avatar?.GetPathInfo((int)req.AvatarId);
        if (avatar == null || path == null)
        {
            await connection.SendPacket(new PacketSetAvatarEnhancedIdScRsp(Retcode.RetAvatarNotExist));
            return;
        }

        path.EnhanceId = (int)req.EnhancedId;
        await connection.Player.SendPacket(new PacketSetAvatarEnhancedIdScRsp(req.AvatarId, path.EnhanceId));
        await connection.Player.SendPacket(new PacketPlayerSyncScNotify(avatar));
    }
}