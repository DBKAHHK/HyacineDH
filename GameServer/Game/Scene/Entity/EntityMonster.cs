using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Config.Scene;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.Scene.Component;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Scene.Entity;

public class EntityMonster(
    SceneInstance scene,
    Position pos,
    Position rot,
    int groupId,
    int instId,
    NPCMonsterDataExcel excel,
    MonsterInfo info) : BaseGameEntity, IGameModifier
{
    public Position Position { get; set; } = pos;
    public Position Rotation { get; set; } = rot;
    public int InstId { get; set; } = instId;
    public SceneInstance Scene { get; set; } = scene;
    public NPCMonsterDataExcel MonsterData { get; set; } = excel;
    public MonsterInfo Info { get; set; } = info;
    public SceneBuff? TempBuff { get; set; }
    public bool IsAlive { get; private set; } = true;

    public int EventId { get; set; } = info.EventID;
    public int CustomStageId { get; set; } = 0;

    public int RogueMonsterId { get; set; } = 0;
    public int CustomLevel { get; set; } = 0;
    public override int EntityId { get; set; } = 0;
    public override int GroupId { get; set; } = groupId;

    public override async ValueTask AddBuff(SceneBuff buff)
    {
        if (!GameData.MazeBuffData.TryGetValue(buff.BuffId * 10 + buff.BuffLevel, out var buffExcel)) return;

        await AddModifier(buffExcel.ModifierName);
        var oldBuff = BuffList.Find(x => x.BuffId == buff.BuffId);
        if (oldBuff != null) BuffList.Remove(oldBuff);
        BuffList.Add(buff);
        await Scene.Player.SendPacket(new PacketSyncEntityBuffChangeListScNotify(this, buff));
    }

    public override async ValueTask ApplyBuff(BattleInstance instance)
    {
        if (TempBuff != null)
        {
            instance.Buffs.Add(new MazeBuff(TempBuff));
            TempBuff = null;
        }

        if (BuffList.Count == 0) return;

        foreach (var buff in BuffList)
        {
            if (buff.IsExpired()) continue;
            instance.Buffs.Add(new MazeBuff(buff));
        }

        await Scene.Player.SendPacket(new PacketSyncEntityBuffChangeListScNotify(this, BuffList));

        BuffList.Clear();
    }

    public override SceneEntityInfo ToProto()
    {
        var proto = new SceneEntityInfo
        {
            EntityId = (uint)EntityId,
            GroupId = (uint)GroupId,
            InstId = (uint)InstId,
            Motion = new MotionInfo
            {
                Pos = Position.ToProto(),
                Rot = Rotation.ToProto()
            },
            NpcMonster = new SceneNpcMonsterInfo
            {
                EventId = (uint)EventId,
                MonsterId = (uint)MonsterData.ID,
                WorldLevel = (uint)Scene.Player.Data.WorldLevel
            }
        };

        if (RogueMonsterId > 0)
            proto.NpcMonster.ExtraInfo = new NpcMonsterExtraInfo
            {
                RogueGameInfo = new NpcMonsterRogueInfo
                {
                    RogueMonsterId = (uint)RogueMonsterId,
                    Level = (uint)CustomLevel
                }
            };

        return proto;
    }

    public List<string> Modifiers { get; set; } = [];

    public async ValueTask AddModifier(string modifierName)
    {
        if (Modifiers.Contains(modifierName)) return;

        GameData.AdventureModifierData.TryGetValue(modifierName, out var modifier);
        GameData.AdventureAbilityConfigListData.TryGetValue(MonsterData.ID, out var ability);
        if (modifier == null || ability == null) return;

        await Scene.Player.TaskManager!.AbilityLevelTask.TriggerTasks(ability, modifier.OnCreate, this, [],
            new SceneCastSkillCsReq());

        Modifiers.Add(modifierName);
    }

    public async ValueTask RemoveModifier(string modifierName)
    {
        if (!Modifiers.Contains(modifierName)) return;

        GameData.AdventureModifierData.TryGetValue(modifierName, out var modifier);
        GameData.AdventureAbilityConfigListData.TryGetValue(MonsterData.ID, out var ability);
        if (modifier == null || ability == null) return;

        await Scene.Player.TaskManager!.AbilityLevelTask.TriggerTasks(ability, modifier.OnDestroy, this, [],
            new SceneCastSkillCsReq());

        Modifiers.Remove(modifierName);
    }

    public async ValueTask RemoveBuff(int buffId)
    {
        if (!GameData.MazeBuffData.TryGetValue(buffId * 10 + 1, out var buffExcel)) return;

        var buff = BuffList.Find(x => x.BuffId == buffId);
        if (buff == null) return;

        BuffList.Remove(buff);
        await Scene.Player.SendPacket(new PacketSyncEntityBuffChangeListScNotify(this, [buff]));

        await RemoveModifier(buffExcel.ModifierName);
    }

    public int GetStageId()
    {
        if (CustomStageId > 0) return CustomStageId;
        return Info.EventID;
    }

    public async ValueTask<List<ItemData>> Kill(bool sendPacket = true)
    {
        IsAlive = false;

        GameData.MonsterDropData.TryGetValue(MonsterData.ID * 10 + Scene.Player.Data.WorldLevel, out var dropData);
        if (dropData == null) return [];
        var dropItems = dropData.CalculateDrop();
        await Scene.Player.InventoryManager!.AddItems(dropItems, sendPacket);

        // TODO: Rogue support
        // call mission handler
        await Scene.Player.MissionManager!.HandleFinishType(MissionFinishTypeEnum.KillMonster, this);
        await Scene.RemoveEntity(this);
        return dropItems;
    }
}