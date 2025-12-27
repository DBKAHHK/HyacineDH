using System.Numerics;
using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.GameServer.Game.Scene.Entity;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.Command.Command.Cmd;

[CommandInfo("scene", "Game.Command.Scene.Desc", "Game.Command.Scene.Usage")]
public class CommandScene : ICommand
{
    [CommandMethod("0 group")]
    public async ValueTask GetLoadedGroup(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var scene = arg.Target!.Player!.SceneInstance!;
        var loadedGroup = new List<int>();
        foreach (var group in scene.Entities)
            if (!loadedGroup.Contains(group.Value.GroupId))
                loadedGroup.Add(group.Value.GroupId);
        await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.LoadedGroups", string.Join(", ", loadedGroup)));
    }

    [CommandMethod("0 prop")]
    public async ValueTask GetProp(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var scene = arg.Target!.Player!.SceneInstance!;
        EntityProp? prop = null;
        foreach (var entity in scene.GetEntitiesInGroup<EntityProp>(arg.GetInt(0)))
            if (entity.PropInfo.ID == arg.GetInt(1))
            {
                prop = entity;
                break;
            }

        if (prop == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.PropNotFound"));
            return;
        }

        await prop.SetState((PropStateEnum)arg.GetInt(2));
        await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.PropStateChanged", prop.PropInfo.ID.ToString(),
            prop.State.ToString()));
    }

    [CommandMethod("0 remove")]
    public async ValueTask RemoveEntity(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var scene = arg.Target!.Player!.SceneInstance!;
        scene.Entities.TryGetValue(arg.GetInt(0), out var entity);
        if (entity == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.EntityNotFound"));
            return;
        }

        await scene.RemoveEntity(entity);
        await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.EntityRemoved", entity.EntityId.ToString()));
    }

    [CommandMethod("0 unlockall")]
    public async ValueTask UnlockAll(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var scene = arg.Target!.Player!.SceneInstance!;
        foreach (var entity in scene.Entities.Values)
            if (entity is EntityProp prop)
                if (prop.Excel.PropStateList.Contains(PropStateEnum.Open))
                    await prop.SetState(PropStateEnum.Open);
        await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.AllPropsUnlocked"));
    }

    [CommandMethod("0 unlockallgroup")]
    public async ValueTask UnlockAllGroup(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var scene = arg.Target!.Player!.SceneInstance!;
        foreach (var groupId in scene.Groups)
        {
            await scene.UpdateGroupProperty(groupId, "Lock", 0);
        }

        foreach (var groupId in scene.Groups)
        {
            await scene.UpdateGroupProperty(groupId, "PlateArrived", 2);
        }

        if (arg.Target.Player.SceneInstance!.FloorId == 20431001)
        {
            // TODO temporary solution
            var savedValueName = "FSV_EnvLight";
            var savedValue = 5;

            // update floor saved data
            if (arg.Target.Player.SceneData!.FloorSavedData.TryGetValue(arg.Target.Player.SceneInstance!.FloorId,
                    out var savedData))
            {
                savedData[savedValueName] = savedValue;
            }
            else
            {
                arg.Target.Player.SceneData!.FloorSavedData[arg.Target.Player.SceneInstance!.FloorId] =
                    new Dictionary<string, int>
                    {
                        { savedValueName, savedValue }
                    };
            }

            // send packet to client
            await arg.Target.Player.SendPacket(
                new PacketUpdateFloorSavedValueNotify(savedValueName, savedValue, arg.Target.Player));
        }

        await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.AllPropsUnlocked"));
    }

    [CommandMethod("0 change")]
    public async ValueTask ChangeScene(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.GetInt(0) == 0)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var player = arg.Target!.Player!;
        await player.EnterScene(arg.GetInt(0), 0, true);
        await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.SceneChanged", arg.GetInt(0).ToString()));
    }

    [CommandMethod("0 reload")]
    public async ValueTask ReloadScene(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var player = arg.Target!.Player!;
        await player.EnterScene(player.Data.EntryId, 0, true);
        await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.SceneReloaded"));
    }

    [CommandMethod("0 reset")]
    public async ValueTask ResetFloor(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var floorId = arg.GetInt(0);
        if (floorId == 0)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var player = arg.Target!.Player!;
        if (player.SceneData?.ScenePropData.TryGetValue(floorId, out _) == true)
            player.SceneData.ScenePropData[floorId] = [];

        if (player.SceneData?.FloorSavedData.TryGetValue(floorId, out _) == true)
            player.SceneData.FloorSavedData[floorId] = [];

        await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.SceneReset", floorId.ToString()));
    }

    [CommandMethod("0 cur")]
    public async ValueTask GetCurrentScene(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var player = arg.Target!.Player!;
        await arg.SendMsg(I18NManager.Translate("Game.Command.Scene.CurrentScene", player.Data.EntryId.ToString(),
            player.Data.PlaneId.ToString(), player.Data.FloorId.ToString()));
    }

    [CommandMethod("0 near")]
    public async ValueTask GetNearestProp(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var player = arg.Target!.Player!;

        var curDistance = 1000000000L;
        EntityProp? nearest = null;
        foreach (var entityProp in player.SceneInstance!.Entities.Values.OfType<EntityProp>())
        {
            var distance = entityProp.Position.GetFast2dDist(player.Data.Pos!);
            if (distance < curDistance)
            {
                nearest = entityProp;
                curDistance = distance;
            }
        }

        if (nearest != null)
            await arg.SendMsg(
                $"Nearest Prop {nearest.EntityId}: PropId {nearest.PropInfo.ID}, GroupId {nearest.GroupId}, State {nearest.State}");
    }

    [CommandMethod("0 forward")]
    public async ValueTask Teleport(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count == 0)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var distance = arg.GetInt(0);
        var player = arg.Target!.Player!;

        var posVec = new Vector3(player.Data.Pos!.X, player.Data.Pos!.Y, player.Data.Pos!.Z);
        var rotVec = new Vector3(player.Data.Rot!.X, player.Data.Rot!.Y, player.Data.Rot!.Z);
        var normalizedVector = Vector3.Normalize(rotVec);

        posVec += normalizedVector * distance;

        // set pos
        await player.MoveTo(new Position((int)posVec.X, (int)posVec.Y, (int)posVec.Z));
        await arg.SendMsg("Teleported!");
    }
}