using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishAction.Handler;

[MissionFinishAction(FinishActionTypeEnum.delMissionItem)]
public class MissionHandlerDelMissionItem : MissionFinishActionHandler
{
    public override async ValueTask OnHandle(List<int> @params, List<string> paramString, PlayerInstance player)
    {
        if (@params.Count < 2) return;
        for (var i = 0; i < @params.Count; i += 2)
        {
            var itemId = @params[i];
            var count = @params[i + 1];
            await player.InventoryManager!.RemoveItem(itemId, count);
        }
    }
}