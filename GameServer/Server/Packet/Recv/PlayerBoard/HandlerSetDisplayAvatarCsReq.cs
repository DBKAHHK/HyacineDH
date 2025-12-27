using EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerBoard;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.PlayerBoard;

[Opcode(CmdIds.SetDisplayAvatarCsReq)]
public class HandlerSetDisplayAvatarCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetDisplayAvatarCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        var avatars = player.AvatarManager!.AvatarData!.DisplayAvatars;
        avatars.Clear();
        foreach (var avatar in req.DisplayAvatarList)
        {
            if (avatar.AvatarId == 0) continue;

            var avatarData = player.AvatarManager!.AvatarData.FormalAvatars.First(x =>
                x.BaseAvatarId == (int)avatar.AvatarId);
            if (avatarData != null) avatars.Add(avatarData.AvatarId);
        }

        await connection.SendPacket(new PacketSetDisplayAvatarScRsp(req.DisplayAvatarList));
    }
}