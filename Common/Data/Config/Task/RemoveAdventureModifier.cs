using Newtonsoft.Json.Linq;

namespace EggLink.DanhengServer.Data.Config.Task;

public class RemoveAdventureModifier : TaskConfigInfo
{
    public TargetEvaluator TargetType { get; set; } = new();
    public string ModifierName { get; set; } = "";

    public new static TaskConfigInfo LoadFromJsonObject(JObject obj)
    {
        var info = new RemoveAdventureModifier
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

        if (obj.TryGetValue(nameof(ModifierName), out value)) info.ModifierName = value.ToObject<string>()!;

        return info;
    }
}