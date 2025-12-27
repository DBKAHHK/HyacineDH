using Newtonsoft.Json.Linq;

namespace EggLink.DanhengServer.Data.Config.Task;

public class AdventureFireProjectile : TaskConfigInfo
{
    public TargetEvaluator TargetType { get; set; } = new();

    //public ProjectileData Projectile { get; set; }
    public List<TaskConfigInfo> OnProjectileHit { get; set; } = [];
    public List<TaskConfigInfo> OnProjectileLifetimeFinish { get; set; } = [];
    public bool WaitProjectileFinish { get; set; }
    public string MutexName { get; set; } = "";

    public new static TaskConfigInfo LoadFromJsonObject(JObject obj)
    {
        var info = new AdventureFireProjectile
        {
            Type = obj[nameof(Type)]!.ToObject<string>()!
        };

        if (obj.TryGetValue(nameof(TargetType), out var value))
        {
            var targetType = value as JObject;
            var classType =
                System.Type.GetType(
                    $"EggLink.DanhengServer.Data.Config.Task.{targetType?["Type"]?.ToString().Replace("RPG.GameCore.", "")}");
            classType ??= System.Type.GetType("EggLink.DanhengServer.Data.Config.Task.TargetEvaluator");
            info.TargetType = (targetType!.ToObject(classType!) as TargetEvaluator)!;
        }

        foreach (var item in
                 obj[nameof(OnProjectileHit)]?.Select(x => TaskConfigInfo.LoadFromJsonObject((x as JObject)!)) ?? [])
            info.OnProjectileHit.Add(item);

        foreach (var item in
                 obj[nameof(OnProjectileLifetimeFinish)]
                     ?.Select(x => TaskConfigInfo.LoadFromJsonObject((x as JObject)!)) ?? [])
            info.OnProjectileLifetimeFinish.Add(item);

        if (obj.TryGetValue(nameof(WaitProjectileFinish), out value))
            info.WaitProjectileFinish = value.ToObject<bool>();

        if (obj.TryGetValue(nameof(MutexName), out value)) info.MutexName = value.ToObject<string>()!;

        return info;
    }
}