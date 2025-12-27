using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishAction.Handler;

[MissionFinishAction(FinishActionTypeEnum.SetGroupProperty)]
public class MissionHandlerSetGroupProperty : MissionFinishActionHandler
{
    public override async ValueTask OnHandle(List<int> @params, List<string> paramString, PlayerInstance player)
    {
        var groupId = paramString[0];
        var propertyName = paramString[1];
        var propertyValue = paramString[2];

        if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(propertyValue))
            return;

        await player.SceneInstance!.UpdateGroupProperty(int.Parse(groupId), propertyName, int.Parse(propertyValue));
    }
}