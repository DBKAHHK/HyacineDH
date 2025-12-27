using EggLink.DanhengServer.Data.Config.Task;
using EggLink.DanhengServer.Database.Quests;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.Enums.Task;
using EggLink.DanhengServer.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Spectre.Console;

namespace EggLink.DanhengServer.Data.Config.Scene;

public class GroupInfo
{
    public int Id;

    [JsonConverter(typeof(StringEnumConverter))]
    public GroupLoadSideEnum LoadSide { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public GroupCategoryEnum Category { get; set; }

    public LevelGroupSystemUnlockCondition? SystemUnlockCondition { get; set; } = null;
    public string LevelGraph { get; set; } = "";
    public bool LoadOnInitial { get; set; }
    public string GroupName { get; set; } = "";
    public SavedValueLoadCondition SavedValueCondition { get; set; } = new();
    public LoadCondition LoadCondition { get; set; } = new();
    public LoadCondition UnloadCondition { get; set; } = new();
    public LoadCondition ForceUnloadCondition { get; set; } = new();

    [JsonConverter(typeof(StringEnumConverter))]
    public SaveTypeEnum SaveType { get; set; } = SaveTypeEnum.Save;

    public int OwnerMainMissionID { get; set; }
    public List<AnchorInfo> AnchorList { get; set; } = [];
    public List<MonsterInfo> MonsterList { get; set; } = [];
    public List<PropInfo> PropList { get; set; } = [];
    public List<NpcInfo> NPCList { get; set; } = [];
    public Dictionary<int, GroupPropertyConfigInfo> GroupPropertyMap { get; set; } = [];
    public ValueSourceInfo? ValueSource { get; set; }

    [JsonIgnore] public LevelGraphConfigInfo? LevelGraphConfig { get; set; }

    [JsonIgnore] public Dictionary<string, List<int>> PropTriggerCustomString { get; set; } = [];
    [JsonIgnore] public List<string> ControlFloorSavedValue { get; set; } = [];
    [JsonIgnore] public List<int> RelatedBattleId { get; set; } = [];

    public void Load()
    {
        foreach (var prop in PropList) prop.Load(this);

        foreach (var source in ValueSource?.Values ?? [])
        {
            if (source["Key"]?.ToString() == "FSV")
            {
                var value = source["Value"];
                if (value != null)
                    ControlFloorSavedValue.Add(value.ToString());
            }
        }

        foreach (var info in LevelGraphConfig?.OnInitSequece ?? [])
        {
            foreach (var configInfo in info.TaskList)
            {
                if (configInfo is TriggerBattle battle)
                {
                    if (battle.EventID.GetValue() > 0)
                        RelatedBattleId.Add(battle.EventID.GetValue());
                }
            }
        }

        foreach (var info in LevelGraphConfig?.OnStartSequece ?? [])
        {
            foreach (var configInfo in info.TaskList)
            {
                if (configInfo is TriggerBattle battle)
                {
                    if (battle.EventID.GetValue() > 0)
                        RelatedBattleId.Add(battle.EventID.GetValue());
                }
            }
        }
    }
}

public class LoadCondition
{
    public List<Condition> Conditions { get; set; } = [];

    [JsonConverter(typeof(StringEnumConverter))]
    public OperationEnum Operation { get; set; } = OperationEnum.And;

    public bool IsTrue(MissionData mission, bool defaultResult = true)
    {
        if (Conditions.Count == 0) return defaultResult;

        // check load condition
        List<Func<bool>> conditionChecks = [];

        foreach (var condition in Conditions)
            if (condition.Type == LevelGroupMissionTypeEnum.MainMission)
            {
                var status = mission.GetMainMissionStatus(condition.ID);
                if (!ConfigManager.Config.ServerOption.EnableMission) status = MissionPhaseEnum.Finish;

                condition.Phase = condition.Phase == MissionPhaseEnum.Cancel
                    ? MissionPhaseEnum.Finish
                    : condition.Phase;

                conditionChecks.Add(CheckFunc);
                continue;

                bool CheckFunc() => status == condition.Phase;
            }
            else
            {
                // sub mission
                var status = mission.GetSubMissionStatus(condition.ID);
                if (!ConfigManager.Config.ServerOption.EnableMission) status = MissionPhaseEnum.Finish;
                condition.Phase = condition.Phase == MissionPhaseEnum.Cancel
                    ? MissionPhaseEnum.Finish
                    : condition.Phase;

                conditionChecks.Add(CheckFunc);
                continue;

                bool CheckFunc() => status == condition.Phase;
            }

        switch (Operation)
        {
            case OperationEnum.And when UtilTools.CheckAnd(conditionChecks, defaultResult):
            case OperationEnum.Or when UtilTools.CheckOr(conditionChecks, defaultResult):
                return true;
            case OperationEnum.Not:
                return !UtilTools.CheckOr(conditionChecks, defaultResult);
            default:
                return defaultResult;
        }
    }
}

public class SavedValueLoadCondition
{
    public List<SavedValueCondition> Conditions { get; set; } = [];

    [JsonConverter(typeof(StringEnumConverter))]
    public OperationEnum Operation { get; set; } = OperationEnum.And;

    public bool IsTrue(Dictionary<string, int> savedValue, bool defaultResult = true)
    {
        if (Conditions.Count == 0) return defaultResult;

        // check load condition
        List<Func<bool>> conditionChecks = [];
        foreach (var condition in Conditions)
        {
            // saved value
            var status = savedValue.GetValueOrDefault(condition.SavedValueName, 0);
            if (condition.Operation == CompareTypeEnum.Unknow) continue;

            conditionChecks.Add(CheckFunc);
            continue;

            bool CheckFunc() => UtilTools.CompareNumberByOperationEnum(status, condition.Value, condition.Operation);
        }

        switch (Operation)
        {
            case OperationEnum.And when UtilTools.CheckAnd(conditionChecks, defaultResult):
            case OperationEnum.Or when UtilTools.CheckOr(conditionChecks, defaultResult):
                return true;
            case OperationEnum.Not:
                return !UtilTools.CheckOr(conditionChecks, defaultResult);
            default:
                return defaultResult;
        }
    }
}

public class Condition
{
    [JsonConverter(typeof(StringEnumConverter))]
    public LevelGroupMissionTypeEnum Type { get; set; } = LevelGroupMissionTypeEnum.MainMission;

    public int ID { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public MissionPhaseEnum Phase { get; set; } = MissionPhaseEnum.Accept;
}

public class SavedValueCondition
{
    public string SavedValueName { get; set; } = "";

    public int Value { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public CompareTypeEnum Operation { get; set; } = CompareTypeEnum.Unknow;
}

public class LevelGroupSystemUnlockCondition
{
    public List<int> Conditions { get; set; } = [];

    [JsonConverter(typeof(StringEnumConverter))]
    public OperationEnum Operation { get; set; }
}

public class GroupPropertyConfigInfo
{
    public int ID { get; set; }
    public string Name { get; set; } = "";

    [JsonConverter(typeof(StringEnumConverter))]
    public GroupPropertySideEnum Side { get; set; }

    public short DefaultValue { get; set; }
    public short MaxValue { get; set; }
    public short MinValue { get; set; }
}