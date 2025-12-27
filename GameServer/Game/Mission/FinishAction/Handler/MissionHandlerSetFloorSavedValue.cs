using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishAction.Handler;

[MissionFinishAction(FinishActionTypeEnum.SetFloorSavedValue)]
public class MissionHandlerSetFloorSavedValue : MissionFinishActionHandler
{
    public override async ValueTask OnHandle(List<int> @params, List<string> paramString, PlayerInstance player)
    {
        _ = int.TryParse(paramString[0], out var plane);
        _ = int.TryParse(paramString[1], out var floor);
        player.SceneData!.FloorSavedData.TryGetValue(floor, out var value);
        if (value == null)
        {
            value = [];
            player.SceneData.FloorSavedData[floor] = value;
        }

        value[paramString[2]] = int.Parse(paramString[3]); // ParamString[2] is the key
        await player.SendPacket(
            new PacketUpdateFloorSavedValueNotify(paramString[2], int.Parse(paramString[3]), player));

        player.TaskManager?.SceneTaskTrigger.TriggerFloor(plane, floor);
    }
}