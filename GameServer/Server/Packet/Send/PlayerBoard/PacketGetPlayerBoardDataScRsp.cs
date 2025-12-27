using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerBoard;

public class PacketGetPlayerBoardDataScRsp : BasePacket
{
    public PacketGetPlayerBoardDataScRsp(PlayerInstance player) : base(CmdIds.GetPlayerBoardDataScRsp)
    {
        var proto = new GetPlayerBoardDataScRsp
        {
            Signature = player.Data.Signature,
            CurrentHeadIconId = (uint)player.Data.HeadIcon,
            // CurrentPersonalCardId 原名为 PersonalCardId
            CurrentPersonalCardId = (uint)player.Data.PersonalCard,
            UnlockedPersonalCardList = { player.PlayerUnlockData!.PersonalCards.Select(x => (uint)x) },
            UnlockedHeadIconList =
                { player.PlayerUnlockData!.HeadIcons.Select(x => new HeadIconData { Id = (uint)x }) },
            AssistAvatarIdList = { player.AvatarManager!.AvatarData.AssistAvatars.Select(x => (uint)x) },
            DisplayAvatarVec = new DisplayAvatarVec()
        };

        var pos = 0;
        player.AvatarManager?.AvatarData!.DisplayAvatars.ForEach(avatar =>
        {
            proto.DisplayAvatarVec.DisplayAvatarList.Add(new DisplayAvatarData
            {
                AvatarId = (uint)avatar,
                Pos = (uint)pos++
            });
        });
        player.AvatarManager?.AvatarData!.AssistAvatars.ForEach(x => proto.AssistAvatarIdList.Add((uint)x));

        SetData(proto);
    }
}