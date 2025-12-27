using EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerBoard;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.PlayerBoard;

[Opcode(CmdIds.SetAssistAvatarCsReq)]
public class HandlerSetAssistAvatarCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetAssistAvatarCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        var avatars = player.AvatarManager!.AvatarData!.AssistAvatars;
        avatars.Clear();
        foreach (var id in req.AvatarIdList)
        {
            if (id == 0) continue;

            var avatarData = player.AvatarManager!.AvatarData.FormalAvatars.First(x =>
                x.BaseAvatarId == (int)id);
            if (avatarData != null) avatars.Add(avatarData.AvatarId);
        }

        await connection.SendPacket(new PacketSetAssistAvatarScRsp(req.AvatarIdList));
    }
}