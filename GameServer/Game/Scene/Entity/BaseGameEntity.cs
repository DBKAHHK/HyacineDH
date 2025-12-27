using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.Scene.Entity;

public abstract class BaseGameEntity
{
    public abstract int EntityId { get; set; }
    public abstract int GroupId { get; set; }

    public List<SceneBuff> BuffList { get; set; } = [];

    public virtual ValueTask AddBuff(SceneBuff buff)
    {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask ApplyBuff(BattleInstance instance)
    {
        return ValueTask.CompletedTask;
    }


    public abstract SceneEntityInfo ToProto();
}