using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Custom;
using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using Newtonsoft.Json.Linq;

namespace EggLink.DanhengServer.GameServer.Game.Scene.Component;

public class RainbowSceneComponent(SceneInstance scene) : BaseSceneComponent(scene)
{
    public int CurTargetPuzzleGroupId { get; set; }

    public override ValueTask Initialize()
    {
        CurTargetPuzzleGroupId =
            SceneInst.Player.SceneData!.FloorTargetPuzzleGroupData.GetValueOrDefault(SceneInst.FloorId, 0);

        SceneInst.GroupPropertyUpdated += GroupPropertyUpdated; 
        return ValueTask.CompletedTask;
    }

    private async ValueTask GroupPropertyUpdated(GroupPropertyRefreshData data)
    {
        var modifiedGroupActions = GameData.SceneRainbowGroupPropertyData.FloorProperty
            .GetValueOrDefault(SceneInst.FloorId, []).GetValueOrDefault(data.GroupId);
        if (modifiedGroupActions == null) return;

        var propertyAction = modifiedGroupActions.GetValueOrDefault(data.PropertyName);

        // get cur actions
        var targetActions = propertyAction?.GetValueOrDefault(data.NewValue);
        if (targetActions == null) return;

        // execute actions
        await ExecuteRainbowActions(targetActions.PrivateActions);
        await ExecuteRainbowActions(targetActions.Actions);
    }

    private async ValueTask ExecuteRainbowActions(List<RainbowActionInfo> actions)
    {
        foreach (var action in actions)
        {
            switch (action.ActionType)
            {
                case SceneActionTypeEnum.Unknown:
                    break;
                case SceneActionTypeEnum.SetGroupProperty:
                    await SetGroupProperty(action.Params);
                    break;
                case SceneActionTypeEnum.ChangeCurrentTargetPuzzle:
                    ChangeCurrentTargetPuzzle(action.Params);
                    break;
                case SceneActionTypeEnum.SetGroupPropertyByCopyAnother:
                    await SetGroupPropertyByCopyAnother(action.Params);
                    break;
                case SceneActionTypeEnum.PropertyValueEqual:
                    await PropertyValueEqual(action.Params);
                    break;
                case SceneActionTypeEnum.SetFloorSavedValue:
                    await SetFloorSavedValue(action.Params);
                    break;
                case SceneActionTypeEnum.CallCurrentTargetPuzzlePropertyAction:
                    await CallCurrentTargetPuzzlePropertyAction(action.Params);
                    break;
                case SceneActionTypeEnum.CallCurrentTargetPuzzlePropertyChanged:
                    await CallCurrentTargetPuzzlePropertyChanged(action.Params);
                    break;
            }
        }
    }

    private async ValueTask SetGroupProperty(Dictionary<string, object> param)
    {
        var groupId = (int)(long)(param.GetValueOrDefault("GroupId") ?? 0);
        var propertyName = (string)(param.GetValueOrDefault("PropertyName") ?? string.Empty);
        var propertyValue = (int)(long)(param.GetValueOrDefault("PropertyValue") ?? 0);

        if (groupId == 0 || string.IsNullOrEmpty(propertyName)) return;

        // update group property
        await SceneInst.UpdateGroupProperty(groupId, propertyName, propertyValue, false);
    }

    private void ChangeCurrentTargetPuzzle(Dictionary<string, object> param)
    {
        var groupId = (int)(long)(param.GetValueOrDefault("GroupId") ?? 0);
        if (groupId == 0) return;

        CurTargetPuzzleGroupId = groupId;
        SceneInst.Player.SceneData!.FloorTargetPuzzleGroupData[SceneInst.FloorId] = groupId;
    }

    private async ValueTask SetGroupPropertyByCopyAnother(Dictionary<string, object> param)
    {
        var groupId = (int)(long)(param.GetValueOrDefault("GroupId") ?? 0);
        var propertyName = (string)(param.GetValueOrDefault("PropertyName") ?? string.Empty);
        var propertyCopyFromName = (string)(param.GetValueOrDefault("PropertyCopyFromName") ?? string.Empty);
        if (groupId == 0 || string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(propertyCopyFromName)) return;

        // get copy from group property
        var copyFromGroupProperty = SceneInst.GetGroupProperty(groupId, propertyCopyFromName);
        await SceneInst.UpdateGroupProperty(groupId, propertyName, copyFromGroupProperty, false);
    }

    private async ValueTask PropertyValueEqual(Dictionary<string, object> param)
    {
        var groupId = (int)(long)(param.GetValueOrDefault("GroupId") ?? 0);
        var propertyName = (string)(param.GetValueOrDefault("PropertyName") ?? string.Empty);
        var propertyValue = (int)(long)(param.GetValueOrDefault("PropertyValue") ?? 0);
        var succActions = (JToken)(param.GetValueOrDefault("SuccActions") ?? JArray.Parse("[]"));
        if (groupId == 0 || string.IsNullOrEmpty(propertyName)) return;

        // cast to List<RainbowActionInfo>
        var actions = succActions.ToObject<List<RainbowActionInfo>>() ?? [];

        // check if group property value equal to target value
        var groupPropertyValue = SceneInst.GetGroupProperty(groupId, propertyName);
        if (groupPropertyValue == propertyValue)
        {
            // execute actions
            await ExecuteRainbowActions(actions);
        }
    }

    private async ValueTask SetFloorSavedValue(Dictionary<string, object> param)
    {
        var savedValueName = (string)(param.GetValueOrDefault("SavedValueName") ?? string.Empty);
        var savedValue = (int)(long)(param.GetValueOrDefault("SavedValue") ?? 0);

        if (string.IsNullOrEmpty(savedValueName)) return;

        // update floor saved data
        await SceneInst.UpdateFloorSavedValue(savedValueName, savedValue);
    }

    private async ValueTask CallCurrentTargetPuzzlePropertyAction(Dictionary<string, object> param)
    {
        var propertyName = (string)(param.GetValueOrDefault("PropertyName") ?? string.Empty);
        
        if (string.IsNullOrEmpty(propertyName)) return;

        // get current target puzzle group actions
        var modifiedGroupActions = GameData.SceneRainbowGroupPropertyData.FloorProperty
            .GetValueOrDefault(SceneInst.FloorId, []).GetValueOrDefault(CurTargetPuzzleGroupId);
        if (modifiedGroupActions == null) return;

        var propertyAction = modifiedGroupActions.GetValueOrDefault(propertyName);

        // get cur actions
        var targetActions = propertyAction?.GetValueOrDefault(SceneInst.GetGroupProperty(CurTargetPuzzleGroupId, propertyName));
        if (targetActions == null) return;

        // execute actions
        await ExecuteRainbowActions(targetActions.Actions);
    }

    private async ValueTask CallCurrentTargetPuzzlePropertyChanged(Dictionary<string, object> param)
    {
        var propertyName = (string)(param.GetValueOrDefault("PropertyName") ?? string.Empty);
        var propertyValue = (int)(long)(param.GetValueOrDefault("PropertyValue") ?? 0);
        
        if (string.IsNullOrEmpty(propertyName)) return;

        // get current target puzzle group actions
        var modifiedGroupActions = GameData.SceneRainbowGroupPropertyData.FloorProperty
            .GetValueOrDefault(SceneInst.FloorId, []).GetValueOrDefault(CurTargetPuzzleGroupId);
        if (modifiedGroupActions == null) return;

        var propertyAction = modifiedGroupActions.GetValueOrDefault(propertyName);

        // get cur actions
        var targetActions = propertyAction?.GetValueOrDefault(propertyValue);
        if (targetActions == null) return;

        // execute actions
        await ExecuteRainbowActions(targetActions.Actions);
    }
}