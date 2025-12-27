using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.SetMultipleAvatarPathsCsReq)]
public class HandlerSetMultipleAvatarPathsCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetMultipleAvatarPathsCsReq.Parser.ParseFrom(data);

        foreach (var targetAvatarType in req.AvatarIdList)
        {
            var avatarId = (int)targetAvatarType;
            var baseAvatarId = connection.Player!.AvatarManager!.GetFormalAvatar(avatarId)!.BaseAvatarId;
            if (baseAvatarId == 8001 && avatarId % 2 == 0) avatarId--;
            await connection.Player!.ChangeAvatarPathType(baseAvatarId, (MultiPathAvatarTypeEnum)avatarId);
        }

        await connection.SendPacket(CmdIds.SetMultipleAvatarPathsScRsp);
    }
}