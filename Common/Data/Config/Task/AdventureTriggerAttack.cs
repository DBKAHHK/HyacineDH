using Newtonsoft.Json.Linq;

namespace EggLink.DanhengServer.Data.Config.Task;

public class AdventureTriggerAttack : TaskConfigInfo
{
    public TargetEvaluator AttackTargetType { get; set; } = new();
    public TargetEvaluator AttackRootTargetType { get; set; } = new();
    public bool TriggerBattle { get; set; }

    public float TriggerBattleDelay { get; set; }

    // public AdventureAttackDetectSummonUnitTriggerConfig SummonUnitTriggerAttackDetectConfig { get; set; }
    // public AdventureAttackDetectShapeConfig AttackDetectConfig { get; set; }
    // public AdventureHitConfig HitConfig { get; set; }
    public List<TaskConfigInfo> OnAttack { get; set; } = [];
    public List<TaskConfigInfo> OnBattle { get; set; } = [];
    public List<TaskConfigInfo> OnHit { get; set; } = [];
    public List<TaskConfigInfo> OnKill { get; set; } = [];
    public bool IncludeProps { get; set; }
    public bool HitTargetFaceToAttacker { get; set; }
    public bool TriggerBattleByAllHitTarget { get; set; }
    public bool AttackDetectCollision { get; set; }


    public new static TaskConfigInfo LoadFromJsonObject(JObject obj)
    {
        var info = new AdventureTriggerAttack
        {
            Type = obj[nameof(Type)]!.ToObject<string>()!
        };
        if (obj.TryGetValue(nameof(AttackTargetType), out var value))
        {
            var targetType = value as JObject;
            var classType =
                System.Type.GetType(
                    $"EggLink.DanhengServer.Data.Config.Task.{targetType?["Type"]?.ToString().Replace("RPG.GameCore.", "")}");
            classType ??= System.Type.GetType("EggLink.DanhengServer.Data.Config.Task.TargetEvaluator");
            info.AttackTargetType = (targetType!.ToObject(classType!) as TargetEvaluator)!;
        }

        if (obj.TryGetValue(nameof(AttackRootTargetType), out var v))
        {
            var targetType = v as JObject;
            var classType =
                System.Type.GetType(
                    $"EggLink.DanhengServer.Data.Config.Task.{targetType?["Type"]?.ToString().Replace("RPG.GameCore.", "")}");
            classType ??= System.Type.GetType("EggLink.DanhengServer.Data.Config.Task.TargetEvaluator");
            info.AttackRootTargetType = (targetType!.ToObject(classType!) as TargetEvaluator)!;
        }

        foreach (var item in
                 obj[nameof(OnAttack)]?.Select(x => TaskConfigInfo.LoadFromJsonObject((x as JObject)!)) ?? [])
            info.OnAttack.Add(item);
        foreach (var item in
                 obj[nameof(OnBattle)]?.Select(x => TaskConfigInfo.LoadFromJsonObject((x as JObject)!)) ?? [])
            info.OnBattle.Add(item);
        foreach (var item in
                 obj[nameof(OnHit)]?.Select(x => TaskConfigInfo.LoadFromJsonObject((x as JObject)!)) ?? [])
            info.OnHit.Add(item);
        foreach (var item in
                 obj[nameof(OnKill)]?.Select(x => TaskConfigInfo.LoadFromJsonObject((x as JObject)!)) ?? [])
            info.OnKill.Add(item);
        if (obj.TryGetValue(nameof(TriggerBattle), out value)) info.TriggerBattle = value.ToObject<bool>();
        if (obj.TryGetValue(nameof(TriggerBattleDelay), out value))
            info.TriggerBattleDelay = value.ToObject<float>();
        if (obj.TryGetValue(nameof(IncludeProps), out value)) info.IncludeProps = value.ToObject<bool>();
        if (obj.TryGetValue(nameof(HitTargetFaceToAttacker), out value))
            info.HitTargetFaceToAttacker = value.ToObject<bool>();
        if (obj.TryGetValue(nameof(TriggerBattleByAllHitTarget), out value))
            info.TriggerBattleByAllHitTarget = value.ToObject<bool>();
        if (obj.TryGetValue(nameof(AttackDetectCollision), out value))
            info.AttackDetectCollision = value.ToObject<bool>();
        return info;
    }
}