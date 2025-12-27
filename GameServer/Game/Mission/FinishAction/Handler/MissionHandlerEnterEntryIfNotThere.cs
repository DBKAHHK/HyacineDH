using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishAction.Handler;

[MissionFinishAction(FinishActionTypeEnum.EnterEntryIfNotThere)]
public class MissionHandlerEnterEntryIfNotThere : MissionFinishActionHandler
{
    public override async ValueTask OnHandle(List<int> @params, List<string> paramString, PlayerInstance player)
    {
        var entryId = @params[0];
        var anchorGroup = @params[1];
        var anchorId = @params[2];

        await player.EnterSceneByEntranceId(entryId, anchorGroup, anchorId, true);
    }
}