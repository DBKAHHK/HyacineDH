using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;

public class PacketGetAvatarDataScRsp : BasePacket
{
    public PacketGetAvatarDataScRsp(PlayerInstance player) : base(CmdIds.GetAvatarDataScRsp)
    {
        var proto = new GetAvatarDataScRsp
        {
            IsGetAll = true
        };

        player.PlayerUnlockData!.Skins.Values.ToList().ForEach(skin =>
            proto.SkinList.AddRange(skin.Select(x => (uint)x)));

        player.AvatarManager?.AvatarData?.FormalAvatars?.ForEach(avatar => { proto.AvatarList.Add(avatar.ToProto()); });

        foreach (var baseAvatarId in GameData.MultiplePathAvatarConfigData.Values.Select(x => x.BaseAvatarID)
                     .ToHashSet())
        {
            var avatar = player.AvatarManager?.GetFormalAvatar(baseAvatarId);
            if (avatar == null) continue;

            proto.CurAvatarPath.Add((uint)avatar.BaseAvatarId, (MultiPathAvatarType)avatar.AvatarId);
            proto.MultiPathAvatarInfoList.AddRange(avatar.ToAvatarPathProto());

            if (baseAvatarId == 8001) proto.BasicTypeIdList.Add((uint)avatar.AvatarId);
        }

        SetData(proto);
    }
}