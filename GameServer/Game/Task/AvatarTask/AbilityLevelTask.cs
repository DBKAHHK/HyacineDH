using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Data.Config.Task;
using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.Enums.RogueMagic;
using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Game.RogueMagic;
using EggLink.DanhengServer.GameServer.Game.Scene;
using EggLink.DanhengServer.GameServer.Game.Scene.Component;
using EggLink.DanhengServer.GameServer.Game.Scene.Entity;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Task.AvatarTask;

public class AbilityLevelTask(PlayerInstance player)
{
    public PlayerInstance Player { get; set; } = player;

    #region Selector

    public List<BaseGameEntity> TargetAlias(TargetEvaluator selector, BaseGameEntity casterEntity,
        List<BaseGameEntity> targetEntities)
    {
        if (selector is TargetAlias target)
            return target.Alias switch
            {
                "Caster" or "ModifierOwnerEntity" => [casterEntity],
                "ParamEntity" or "AllEnemy" or "AbilityTargetEntity" => targetEntities,
                _ => targetEntities
            };

        return [];
    }

    #endregion

    #region Manage

    public async ValueTask<AbilityLevelResult> TriggerTasks(AdventureAbilityConfigListInfo abilities,
        List<TaskConfigInfo> tasks, BaseGameEntity casterEntity, List<BaseGameEntity> targetEntities, SceneCastSkillCsReq req,
        string? modifierName = null)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];
        foreach (var task in tasks)
            try
            {
                var res = await TriggerTask(new AbilityLevelParam(abilities, task, casterEntity, targetEntities, req,
                    modifierName));
                if (res.BattleInfos != null) battleInfos.AddRange(res.BattleInfos);

                if (res.Instance != null) instance = res.Instance;
            }
            catch (Exception e)
            {
                Logger.GetByClassName().Error("An error occured, ", e);
            }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> TriggerTask(AbilityLevelParam param)
    {
        try
        {
            var methodName = param.Act.Type.Replace("RPG.GameCore.", "");

            var method = GetType().GetMethod(methodName);
            if (method != null)
            {
                var res = method.Invoke(this, [param]);
                if (res is AbilityLevelResult result) return result;

                if (res is ValueTask<AbilityLevelResult> valueTask) return await valueTask;
            }
        }
        catch (Exception e)
        {
            Logger.GetByClassName().Error("An error occured, ", e);
        }

        return new AbilityLevelResult();
    }

    #endregion

    #region Task

    public async ValueTask<AbilityLevelResult> PredicateTaskList(AbilityLevelParam param)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];

        if (param.Act is PredicateTaskList predicateTaskList)
        {
            // handle predicateCondition
            var methodName = predicateTaskList.Predicate.Type.Replace("RPG.GameCore.", "");
            var method = GetType().GetMethod(methodName);
            if (method != null)
            {
                var resp = method.Invoke(this, [param with { Act = predicateTaskList.Predicate }]);
                if (resp is not bool res) return new AbilityLevelResult(instance, battleInfos);

                res = predicateTaskList.Predicate.Inverse ? !res : res;
                if (res)
                    foreach (var task in predicateTaskList.SuccessTaskList)
                    {
                        var result = await TriggerTask(param with { Act = task });
                        if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                        if (result.Instance != null) instance = result.Instance;
                    }
                else
                    foreach (var task in predicateTaskList.FailedTaskList)
                    {
                        var result = await TriggerTask(param with { Act = task });
                        if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                        if (result.Instance != null) instance = result.Instance;
                    }
            }
            else
            {
                foreach (var task in predicateTaskList.SuccessTaskList)
                {
                    var result = await TriggerTask(param with { Act = task });
                    if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                    if (result.Instance != null) instance = result.Instance;
                }
            }
        }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> AdventureTriggerAttack(AbilityLevelParam param)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];

        if (param.Act is AdventureTriggerAttack adventureTriggerAttack)
        {
            var methodName = adventureTriggerAttack.AttackTargetType.Type.Replace("RPG.GameCore.", "");
            var method = GetType().GetMethod(methodName);
            if (method != null)
            {
                var resp = method.Invoke(this,
                    [adventureTriggerAttack.AttackTargetType, param.CasterEntity, param.TargetEntities]);
                if (resp is List<BaseGameEntity> target)
                {
                    foreach (var task in adventureTriggerAttack.OnAttack)
                    {
                        var result = await TriggerTask(param with { Act = task });
                        if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);
                    }

                    if (target.Count > 0 && adventureTriggerAttack.TriggerBattle)
                    {
                        foreach (var task in adventureTriggerAttack.OnBattle)
                        {
                            var result = await TriggerTask(param with { Act = task });
                            if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);
                        }

                        foreach (var entity in param.TargetEntities)
                        {
                            var type = MonsterBattleType.TriggerBattle;
                            if (entity is EntityMonster { IsAlive: false })
                                type = MonsterBattleType.DirectDieSkipBattle;

                            battleInfos.Add(new HitMonsterInstance(entity.EntityId, type));
                        }

                        instance = await Player.BattleManager!.StartBattle(param.CasterEntity, param.TargetEntities,
                            param.Request.SkillIndex == 1);
                    }
                }
            }
        }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> AddMazeBuff(AbilityLevelParam param)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];

        if (param.Act is AddMazeBuff addMazeBuff)
        {
            var methodName = addMazeBuff.TargetType.Type.Replace("RPG.GameCore.", "");
            var method = GetType().GetMethod(methodName);
            if (method != null)
            {
                var resp = method.Invoke(this,
                    [addMazeBuff.TargetType, param.CasterEntity, param.TargetEntities]);

                Dictionary<string, float> dynamic = [];
                foreach (var dynamicValue in addMazeBuff.DynamicValues)
                    dynamic.Add(dynamicValue.Key, dynamicValue.Value.GetValue());

                if (resp is not List<BaseGameEntity> target) return new AbilityLevelResult(instance, battleInfos);

                foreach (var entity in target)
                    await entity.AddBuff(new SceneBuff(addMazeBuff.ID, 1,
                        (param.CasterEntity as AvatarSceneInfo)?.AvatarInfo.BaseAvatarId ?? 0,
                        addMazeBuff.LifeTime.FixedValue.Value < -1 ? 20 : -1)
                    {
                        DynamicValues = dynamic
                    });
            }
        }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> AdventureFireProjectile(AbilityLevelParam param)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];

        if (param.Act is AdventureFireProjectile adventureFireProjectile)
        {
            foreach (var task in adventureFireProjectile.OnProjectileHit)
            {
                var result = await TriggerTask(param with { Act = task });
                if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                if (result.Instance != null)
                    instance = result.Instance;
            }

            foreach (var task in adventureFireProjectile.OnProjectileLifetimeFinish)
            {
                var result = await TriggerTask(param with { Act = task });
                if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                if (result.Instance != null)
                    instance = result.Instance;
            }
        }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> NewAdventureFireProjectile(AbilityLevelParam param)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];

        if (param.Act is AdventureFireProjectile adventureFireProjectile)
        {
            foreach (var task in adventureFireProjectile.OnProjectileHit)
            {
                var result = await TriggerTask(param with { Act = task });
                if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                if (result.Instance != null)
                    instance = result.Instance;
            }

            foreach (var task in adventureFireProjectile.OnProjectileLifetimeFinish)
            {
                var result = await TriggerTask(param with { Act = task });
                if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                if (result.Instance != null)
                    instance = result.Instance;
            }
        }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> CreateSummonUnit(AbilityLevelParam param)
    {
        if (param.Act is CreateSummonUnit createSummonUnit)
        {
            if (!GameData.SummonUnitDataData.TryGetValue(createSummonUnit.SummonUnitID, out var excel))
                return new AbilityLevelResult();

            if (excel.IsClient) return new AbilityLevelResult();

            var unit = new EntitySummonUnit
            {
                EntityId = 0,
                CreateAvatarEntityId = param.CasterEntity.EntityId,
                AttachEntityId = excel.ConfigInfo?.AttachPoint == "Origin" ? param.CasterEntity.EntityId : 0,
                SummonUnitId = excel.ID,
                CreateAvatarId = (param.CasterEntity as AvatarSceneInfo)?.AvatarInfo.BaseAvatarId ?? 0,
                LifeTimeMs = createSummonUnit.Duration.FixedValue.Value == -1 ? -1 : 20000,
                TriggerList = excel.ConfigInfo?.TriggerConfig.CustomTriggers ?? [],
                Motion = param.Request.TargetMotion
            };

            await Player.SceneInstance!.AddSummonUnitEntity(unit);
        }

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> DestroySummonUnit(AbilityLevelParam param)
    {
        if (param.Act is DestroySummonUnit destroySummonUnit)
            await Player.SceneInstance!.RemoveSummonUnitById(destroySummonUnit.SummonUnit.SummonUnitID); // TODO

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> AddAdventureModifier(AbilityLevelParam param)
    {
        if (param.Act is AddAdventureModifier addAdventureModifier)
        {
            GameData.AdventureModifierData.TryGetValue(addAdventureModifier.ModifierName, out var modifier);
            if (modifier == null) return new AbilityLevelResult();

            if (param.CasterEntity is IGameModifier mod) await mod.AddModifier(addAdventureModifier.ModifierName);
        }

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> RemoveAdventureModifier(AbilityLevelParam param)
    {
        if (param.Act is RemoveAdventureModifier removeAdventureModifier)
        {
            GameData.AdventureModifierData.TryGetValue(removeAdventureModifier.ModifierName, out var modifier);
            if (modifier == null) return new AbilityLevelResult();

            if (param.CasterEntity is IGameModifier mod) await mod.RemoveModifier(removeAdventureModifier.ModifierName);
        }

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> RemoveSelfModifier(AbilityLevelParam param)
    {
        if (param.ModifierName != null)
            if (param.CasterEntity is IGameModifier mod)
                await mod.RemoveModifier(param.ModifierName);

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> RefreshMazeBuffTime(AbilityLevelParam param)
    {
        if (param.Act is RefreshMazeBuffTime refreshMazeBuffTime)
        {
            // get buff
            var buff = param.CasterEntity.BuffList.FirstOrDefault(x => x.BuffId == refreshMazeBuffTime.ID);
            if (buff == null) return new AbilityLevelResult();
            buff.Duration = refreshMazeBuffTime.LifeTime.GetValue();
            await Player.SendPacket(new PacketSyncEntityBuffChangeListScNotify(param.CasterEntity, buff));
        }

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> AdvModifyMaxMazeMP(AbilityLevelParam param)
    {
        await ValueTask.CompletedTask;

        if (param.Act is AdvModifyMaxMazeMP advModifyMaxMazeMp)
            switch (advModifyMaxMazeMp.ModifyFunction)
            {
                case PropertyModifyFunctionEnum.Add:
                    Player.LineupManager!.LineupData.ExtraMpCount += advModifyMaxMazeMp.ModifyValue.GetValue();
                    break;
                case PropertyModifyFunctionEnum.Set:
                    Player.LineupManager!.LineupData.ExtraMpCount = advModifyMaxMazeMp.ModifyValue.GetValue() - 5;
                    break;
            }

        return new AbilityLevelResult();
    }

    public async ValueTask AdventureSetAttackTargetMonsterDie(AbilityLevelParam param)
    {
        var avatar = param.CasterEntity as AvatarSceneInfo;
        if (GameData.AvatarConfigData.TryGetValue(avatar?.AvatarInfo.AvatarId ?? 0, out var excel))
        {
            var adventurePlayerExcel = GameData.AdventurePlayerData.GetValueOrDefault(excel.AdventurePlayerID);
            if (adventurePlayerExcel != null && adventurePlayerExcel.MazeSkillIdList.Count > param.Request.SkillIndex)
            {
                var skill = GameData.MazeSkillData.GetValueOrDefault(
                    adventurePlayerExcel.MazeSkillIdList[(int)param.Request.SkillIndex]);

                await Player.LineupManager!.CostMp(skill?.MPCost ?? 1, param.Request.CastEntityId);
            }
        }

        foreach (var targetEntity in param.TargetEntities)
        {
            if (targetEntity is not EntityMonster monster) continue;

            if (monster.MonsterData.Rank < MonsterRankEnum.Elite)
            {
                await monster.Kill();

                var instance = monster.Scene.Player.RogueManager!.GetRogueInstance();
                switch (instance)
                {
                    case null:
                        continue;
                    case RogueMagicInstance magic:
                        await magic.RollMagicUnit(1, 1, [RogueMagicUnitCategoryEnum.Common]);
                        break;
                    default:
                        await instance.RollBuff(1);
                        break;
                }

                await instance.GainMoney(Random.Shared.Next(20, 60));
            }
        }
    }

    #endregion

    #region Predicate

    public bool ByAllowInstantKill(AbilityLevelParam param)
    {
        foreach (var targetEntity in param.TargetEntities)
            if (targetEntity is EntityMonster monster)
                if (monster.MonsterData.Rank < MonsterRankEnum.Elite)
                    return true;

        return false;
    }

    public bool ByIsContainAdventureModifier(AbilityLevelParam param)
    {
        if (param.Act is ByIsContainAdventureModifier byIsContain)
        {
            // get target
            var result = false;
            var methodName = byIsContain.TargetType.Type.Replace("RPG.GameCore.", "");
            var method = GetType().GetMethod(methodName);
            if (method != null)
            {
                var resp = method.Invoke(this,
                    [byIsContain.TargetType, param.CasterEntity, param.TargetEntities]);

                if (resp is List<BaseGameEntity> target)
                    foreach (var entity in target)
                    {
                        if (entity is not IGameModifier modifier) continue;
                        if (modifier.Modifiers.Contains(byIsContain.ModifierName))
                        {
                            result = true;
                            break;
                        }
                    }
            }

            return result;
        }

        return false;
    }

    public bool AdventureByInMotionState(AbilityLevelParam param)
    {
        return true;
    }

    public bool AdventureByPlayerCurrentSkillType(AbilityLevelParam param)
    {
        if (param.Act is AdventureByPlayerCurrentSkillType byPlayerCurrentSkillType)
            return param.Request.SkillIndex == (uint)byPlayerCurrentSkillType.SkillType;

        return false;
    }

    public bool ByCompareCarryMazebuff(AbilityLevelParam param)
    {
        if (param.Act is ByCompareCarryMazebuff byCompareCarryMazebuff)
            return param.CasterEntity.BuffList.Any(x => x.BuffId == byCompareCarryMazebuff.BuffID);

        return false;
    }

    public bool ByAnd(AbilityLevelParam param)
    {
        if (param.Act is ByAnd byAnd)
        {
            foreach (var task in byAnd.PredicateList)
            {
                var methodName = task.Type.Replace("RPG.GameCore.", "");
                var method = GetType().GetMethod(methodName);
                if (method != null)
                {
                    var resp = method.Invoke(this, [param with { Act = task }]);
                    if (resp is not bool res) return false;
                    res = task.Inverse ? !res : res;
                    if (!res) return false;
                }
            }

            return true;
        }

        return false;
    }

    #endregion
}

public record AbilityLevelResult(BattleInstance? Instance = null, List<HitMonsterInstance>? BattleInfos = null);

public record AbilityLevelParam(
    AdventureAbilityConfigListInfo AdventureAbility,
    TaskConfigInfo Act,
    BaseGameEntity CasterEntity,
    List<BaseGameEntity> TargetEntities,
    SceneCastSkillCsReq Request,
    string? ModifierName);