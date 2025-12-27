using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishAction.Handler;

[MissionFinishAction(FinishActionTypeEnum.ChangeLineup)]
public class MissionHandlerChangeLineup : MissionFinishActionHandler
{
    public override async ValueTask OnHandle(List<int> @params, List<string> paramString, PlayerInstance player)
    {
        player.LineupManager!.GetCurLineup()!.BaseAvatars!.Clear();
        var count = 0;
        var avatarCount = @params.Count(value => value != 0) - 1;
        foreach (var avatarId in @params)
        {
            if (count++ >= 4) break;
            GameData.SpecialAvatarData.TryGetValue(avatarId * 10 + player.Data.WorldLevel, out var specialAvatar);
            if (specialAvatar == null)
            {
                GameData.AvatarConfigData.TryGetValue(avatarId, out var avatar);
                if (avatar == null) continue;
                var ava = player.AvatarManager!.GetFormalAvatar(avatarId);
                if (ava == null) await player.AvatarManager!.AddAvatar(avatarId);
                await player.LineupManager!.AddAvatarToCurTeam(avatarId, count == avatarCount);
            }
            else
            {
                await player.LineupManager!.AddSpecialAvatarToCurTeam(avatarId * 10 + player.Data.WorldLevel,
                    count == avatarCount);
            }
        }

        GameData.SpecialAvatarData.TryGetValue(@params[4] * 10 + player.Data.WorldLevel, out var leaderAvatar);
        if (leaderAvatar == null)
            player.LineupManager!.GetCurLineup()!.LeaderAvatarId = @params[4];
        else
            player.LineupManager!.GetCurLineup()!.LeaderAvatarId = leaderAvatar.AvatarID;

        await player.SceneInstance!.SyncLineup();
    }
}